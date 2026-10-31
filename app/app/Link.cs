using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace CapriDesktop
{
    // What the bridge asks the running Capri: the link code from their Claude setup, her tool and its arguments.
    [DataContract]
    internal sealed class LinkCall
    {
        [DataMember(Name = "token")] public string Token;
        [DataMember(Name = "tool")] public string Tool;
        [DataMember(Name = "args")] public ToolArguments Args;
    }

    [DataContract]
    internal sealed class LinkReply
    {
        [DataMember(Name = "text")] public string Text;
    }

    // The Claude link: "Capri X.exe --mcp <code>" is a small MCP server Claude starts on this PC. It speaks MCP over
    // stdin and stdout and passes each tool call to the running Capri through a pipe only this Windows user can open.
    // No API key: Claude calls her tools from the user's own Claude app, and she only does what Settings lets it.
    internal static class Link
    {
        internal static readonly string PipeName = "CapriFixed.Link." + WindowsIdentity.GetCurrent().User.Value;

        // Her tools as Claude sees them, each with the group that turns it on or off in Settings.
        internal static readonly string[,] Tools =
        {
            { "capri_say", "talk", "Make Capri say something out loud in her own voice, with her speech bubble. Write it as she'd say it: a short line from a cyberpunk netrunner who calls the user choom.",
              "{\"type\":\"object\",\"properties\":{\"text\":{\"type\":\"string\",\"description\":\"What she says, one or two short sentences\"}},\"required\":[\"text\"]}" },
            { "capri_look", "screen", "Have Capri take a screenshot of the user's screen and describe what's on it.",
              "{\"type\":\"object\",\"properties\":{\"focus\":{\"type\":\"string\",\"description\":\"What to look for, if anything\"}}}" },
            { "capri_play", "music", "Have Capri play a song, playlist, album or artist on Spotify, or a video on YouTube.",
              "{\"type\":\"object\",\"properties\":{\"song\":{\"type\":\"string\",\"description\":\"What to play, like a song and its artist, or a video topic\"},\"app\":{\"type\":\"string\",\"enum\":[\"spotify\",\"youtube\"],\"description\":\"Where to play it, if the user said\"},\"kind\":{\"type\":\"string\",\"enum\":[\"song\",\"playlist\",\"album\",\"artist\"]}},\"required\":[\"song\"]}" },
            { "capri_media", "music", "Have Capri pause, skip, go back, change the volume or say what's playing.",
              "{\"type\":\"object\",\"properties\":{\"action\":{\"type\":\"string\",\"enum\":[\"play_pause\",\"next\",\"previous\",\"volume_up\",\"volume_down\",\"volume_set\",\"mute\",\"unmute\",\"now_playing\"]},\"level\":{\"type\":\"string\",\"description\":\"For volume_set, 0 to 100\"}},\"required\":[\"action\"]}" },
            { "capri_open", "apps", "Have Capri open an app on the user's PC, or a website (optionally searching it).",
              "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\",\"description\":\"An app's name, like Discord or Spotify\"},\"site\":{\"type\":\"string\",\"description\":\"A website, like youtube.com, instead of an app\"},\"search\":{\"type\":\"string\",\"description\":\"What to search on that site\"}}}" },
            { "capri_notes", "memory", "Read Capri's notes about the user (me.md): their name, favourites, projects and things they asked her to remember.",
              "{\"type\":\"object\",\"properties\":{}}" },
            { "capri_remember", "memory", "Add a note to Capri's memory about the user, or forget one.",
              "{\"type\":\"object\",\"properties\":{\"text\":{\"type\":\"string\",\"description\":\"The note, about the user\"},\"forget\":{\"type\":\"boolean\",\"description\":\"True to forget notes about this instead\"}},\"required\":[\"text\"]}" },
            { "capri_do", "hands", "Have Capri do a multi-step job with her own mouse cursor and keyboard, like sending a Discord message or searching YouTube and picking a video. The user may be asked to allow it first.",
              "{\"type\":\"object\",\"properties\":{\"task\":{\"type\":\"string\",\"description\":\"The whole job in plain words\"}},\"required\":[\"task\"]}" },
        };

        internal static readonly string[,] Groups = { { "talk", "Talk" }, { "screen", "See my screen" }, { "music", "Music" }, { "apps", "Open apps" }, { "memory", "Her notes" }, { "hands", "Mouse & keyboard" } };

        // Mouse and keyboard stay off until they turn it on.
        internal const string DefaultOff = "hands";

        internal static string GroupOf(string tool)
        {
            for (int i = 0; i < Tools.GetLength(0); i++) if (Tools[i, 0] == tool) return Tools[i, 1];
            return null;
        }

        internal static string NewCode()
        {
            byte[] random = new byte[16];
            using (System.Security.Cryptography.RandomNumberGenerator source = System.Security.Cryptography.RandomNumberGenerator.Create()) source.GetBytes(random);
            return BitConverter.ToString(random).Replace("-", "").ToLowerInvariant();
        }

        // Her side: one call at a time, from this Windows user only.
        internal static void Serve(Func<LinkCall, string> answer, Func<bool> stopped)
        {
            Thread thread = new Thread(delegate()
            {
                while (!stopped())
                {
                    try
                    {
                        PipeSecurity security = new PipeSecurity();
                        security.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().User, PipeAccessRights.FullControl, AccessControlType.Allow));
                        using (NamedPipeServerStream pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 4096, 4096, security))
                        {
                            pipe.WaitForConnection();
                            LinkCall call = null;
                            try { call = (LinkCall)new DataContractJsonSerializer(typeof(LinkCall)).ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(ReadLine(pipe) ?? ""))); }
                            catch (SerializationException) { }
                            string text;
                            try { text = call == null ? "Capri couldn't read that request." : answer(call); }
                            catch (Exception error) { text = "That didn't work: " + error.Message; }
                            MemoryStream reply = new MemoryStream();
                            new DataContractJsonSerializer(typeof(LinkReply)).WriteObject(reply, new LinkReply { Text = text ?? "" });
                            reply.WriteByte((byte)'\n');
                            pipe.Write(reply.ToArray(), 0, (int)reply.Length);
                            pipe.Flush();
                            pipe.WaitForPipeDrain();
                        }
                    }
                    catch (IOException) { Thread.Sleep(200); }
                    catch (UnauthorizedAccessException) { Thread.Sleep(2000); }
                }
            });
            thread.IsBackground = true;
            thread.Name = "Claude link";
            thread.Start();
        }

        private static string ReadLine(Stream stream)
        {
            List<byte> line = new List<byte>();
            int next;
            while ((next = stream.ReadByte()) >= 0 && next != '\n') if (line.Count < 1 << 20) line.Add((byte)next);
            return next < 0 && line.Count == 0 ? null : Encoding.UTF8.GetString(line.ToArray());
        }

        // The bridge Claude runs: MCP's JSON-RPC over stdin and stdout, a message a line.
        internal static int Bridge(string code)
        {
            JavaScriptSerializer json = new JavaScriptSerializer();
            Stream output = Console.OpenStandardOutput();
            using (StreamReader input = new StreamReader(Console.OpenStandardInput(), new UTF8Encoding(false)))
            {
                string line;
                while ((line = input.ReadLine()) != null)
                {
                    if (line.Trim().Length == 0) continue;
                    Dictionary<string, object> message;
                    try { message = json.DeserializeObject(line) as Dictionary<string, object>; }
                    catch (ArgumentException) { message = null; }
                    if (message == null) { Write(output, json, Error(null, -32700, "Parse error")); continue; }
                    object id;
                    bool asked = message.TryGetValue("id", out id);
                    string method = message.ContainsKey("method") ? message["method"] as string : null;
                    Dictionary<string, object> parameters = message.ContainsKey("params") ? message["params"] as Dictionary<string, object> : null;
                    // Notifications get no answer.
                    if (!asked || method == null) continue;
                    Dictionary<string, object> result;
                    if (method == "initialize")
                    {
                        object version = null;
                        if (parameters != null) parameters.TryGetValue("protocolVersion", out version);
                        result = new Dictionary<string, object>
                        {
                            { "protocolVersion", version as string ?? "2025-06-18" },
                            { "capabilities", new Dictionary<string, object> { { "tools", new Dictionary<string, object>() } } },
                            { "serverInfo", new Dictionary<string, object> { { "name", "capri" }, { "version", typeof(Link).Assembly.GetName().Version.ToString(3) } } },
                            { "instructions", "Capri is the user's desktop companion on their Windows PC, a cyberpunk netrunner girl. These tools make the running Capri app talk, look at their screen, play music, open apps, use her notes about them, or do jobs with her own mouse and keyboard. The user picks which of these Capri allows in her settings." }
                        };
                    }
                    else if (method == "tools/list") result = new Dictionary<string, object> { { "tools", ToolList(json) } };
                    else if (method == "tools/call")
                    {
                        object name = null, arguments = null;
                        if (parameters != null) { parameters.TryGetValue("name", out name); parameters.TryGetValue("arguments", out arguments); }
                        result = new Dictionary<string, object>
                        {
                            { "content", new object[] { new Dictionary<string, object> { { "type", "text" }, { "text", Call(json, code, name as string, arguments as Dictionary<string, object>) } } } }
                        };
                    }
                    else if (method == "ping") result = new Dictionary<string, object>();
                    else { Write(output, json, Error(id, -32601, "Method not found: " + method)); continue; }
                    Write(output, json, new Dictionary<string, object> { { "jsonrpc", "2.0" }, { "id", id }, { "result", result } });
                }
            }
            return 0;
        }

        private static object[] ToolList(JavaScriptSerializer json)
        {
            object[] tools = new object[Tools.GetLength(0)];
            for (int i = 0; i < tools.Length; i++)
                tools[i] = new Dictionary<string, object> { { "name", Tools[i, 0] }, { "description", Tools[i, 2] }, { "inputSchema", json.DeserializeObject(Tools[i, 3]) } };
            return tools;
        }

        private static Dictionary<string, object> Error(object id, int code, string text)
        {
            return new Dictionary<string, object> { { "jsonrpc", "2.0" }, { "id", id }, { "error", new Dictionary<string, object> { { "code", code }, { "message", text } } } };
        }

        private static void Write(Stream output, JavaScriptSerializer json, object message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json.Serialize(message) + "\n");
            output.Write(bytes, 0, bytes.Length);
            output.Flush();
        }

        // One tool call, passed to the running Capri. Every argument goes as text, the way her own tools take them.
        private static string Call(JavaScriptSerializer json, string code, string tool, Dictionary<string, object> arguments)
        {
            if (GroupOf(tool) == null) return "There's no Capri tool called " + tool + ".";
            Dictionary<string, object> args = new Dictionary<string, object>();
            if (arguments != null)
                foreach (KeyValuePair<string, object> pair in arguments)
                    if (pair.Value != null && !(pair.Value is Dictionary<string, object>) && !(pair.Value is object[])) args[pair.Key] = Convert.ToString(pair.Value, System.Globalization.CultureInfo.InvariantCulture);
            object forget;
            if (tool == "capri_remember" && args.TryGetValue("forget", out forget) && string.Equals(forget as string, "true", StringComparison.OrdinalIgnoreCase)) args["action"] = "forget";
            args.Remove("forget");
            string request = json.Serialize(new Dictionary<string, object> { { "token", code ?? "" }, { "tool", tool }, { "args", args } }) + "\n";
            try
            {
                using (NamedPipeClientStream pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut))
                {
                    pipe.Connect(3000);
                    byte[] bytes = Encoding.UTF8.GetBytes(request);
                    pipe.Write(bytes, 0, bytes.Length);
                    pipe.Flush();
                    string reply = ReadLine(pipe);
                    Dictionary<string, object> answer = reply == null ? null : json.DeserializeObject(reply) as Dictionary<string, object>;
                    object text;
                    return answer != null && answer.TryGetValue("text", out text) && text is string ? (string)text : "Capri didn't answer.";
                }
            }
            catch (TimeoutException) { return "Capri isn't running, or her Claude link is off. Open Capri X and turn on Claude link in Capri Mainframe > Settings."; }
            catch (IOException error) { return "Couldn't reach Capri: " + error.Message; }
            catch (UnauthorizedAccessException) { return "Couldn't reach Capri: this Windows user can't open her link."; }
        }
    }
}
