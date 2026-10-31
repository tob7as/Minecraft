using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;

namespace CapriDesktop
{
    // The Minecraft link in More settings. Bedrock's /connect talks to a websocket server: either she hosts one on this PC
    // (the address is a port, and they type "/connect localhost:<port>" in game), or she connects out to their own websocket
    // (a ws:// or wss:// address, like a relay or bridge they run) that speaks the same Minecraft messages. Either way she hears
    // chat and a few game events, and talks back in chat with tellraw, which works with or without the Capri Companion add-on.
    internal sealed class MinecraftLink
    {
        internal const int DefaultPort = 19131;
        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();
        private static readonly string[] Events = { "PlayerMessage", "PlayerDied", "MobKilled" };

        // Chat: sender, message. Game: event name and its body. Status: a short line for More settings.
        internal event Action<string, string> Chat;
        internal event Action<string, System.Collections.Generic.Dictionary<string, object>> Game;
        internal event Action<string> Status;
        // The Capri Companion add-on's feed (her state and what just happened), read with "/capri feed" about once a second.
        internal event Action<System.Collections.Generic.Dictionary<string, object>> Feed;

        private volatile int run;
        private readonly System.Collections.Generic.Dictionary<string, Action<System.Collections.Generic.Dictionary<string, object>>> waiting =
            new System.Collections.Generic.Dictionary<string, Action<System.Collections.Generic.Dictionary<string, object>>>();
        private DateTime lastFeed = DateTime.MinValue;
        // True while the add-on answers the feed: then her body says its own lines in chat, so the app doesn't repeat them.
        internal bool FeedLive { get { return send != null && (DateTime.UtcNow - lastFeed).TotalSeconds < 5; } }
        private readonly object sending = new object();
        private Func<string, bool> send;
        internal string Shown { get; private set; }
        internal bool Connected { get { return send != null; } }

        // "19131" or "localhost:19131" hosts here; "ws://..." or "wss://..." connects out. Null if it's neither.
        internal static string Check(string address)
        {
            address = (address ?? "").Trim();
            if (address.Length == 0) return DefaultPort.ToString();
            if (Regex.IsMatch(address, @"^wss?://\S+$", RegexOptions.IgnoreCase)) return Uri.IsWellFormedUriString(address, UriKind.Absolute) ? address : null;
            Match port = Regex.Match(address, @"^(?:(?:localhost|127\.0\.0\.1):)?(\d{2,5})$");
            if (port.Success && int.Parse(port.Groups[1].Value) >= 1024 && int.Parse(port.Groups[1].Value) <= 65535) return port.Groups[1].Value;
            return null;
        }

        internal void Start(string address)
        {
            int mine = ++run;
            Drop();
            Thread worker = new Thread(delegate() { if (address.Contains("://")) ConnectOut(mine, address); else Host(mine, int.Parse(address)); });
            worker.IsBackground = true;
            worker.Name = "Minecraft link";
            worker.Start();
            Thread poll = new Thread(delegate() { Poll(mine); });
            poll.IsBackground = true;
            poll.Name = "Minecraft feed";
            poll.Start();
        }

        internal void Stop() { run++; Drop(); Report("Minecraft link off."); }

        private void Drop() { lock (sending) send = null; lock (waiting) waiting.Clear(); lastFeed = DateTime.MinValue; }

        private void Report(string line) { Shown = line; Action<string> heard = Status; if (heard != null) heard(line); }

        // ---------------------------------------------------------------- what goes to Minecraft
        internal bool Command(string line)
        {
            Func<string, bool> to;
            lock (sending) to = send;
            if (to == null) return false;
            var request = new System.Collections.Generic.Dictionary<string, object>
            {
                { "header", Header("commandRequest") },
                { "body", new System.Collections.Generic.Dictionary<string, object> { { "version", 1 }, { "commandLine", line }, { "origin", new System.Collections.Generic.Dictionary<string, object> { { "type", "player" } } } } }
            };
            return to(Json.Serialize(request));
        }

        // Her line in chat, pink like the add-on's: "<Capri> ...".
        internal bool Say(string text)
        {
            text = Regex.Replace(text ?? "", @"\s+", " ").Trim();
            if (text.Length == 0) return false;
            if (text.Length > 400) text = text.Substring(0, 397) + "...";
            string raw = Json.Serialize(new { rawtext = new[] { new { text = "§d<Capri>§r " + text } } });
            return Command("tellraw @a " + raw);
        }

        // A command whose answer matters: the game's commandResponse (same requestId) goes to `reply`, on the link's thread.
        internal bool Ask(string line, Action<System.Collections.Generic.Dictionary<string, object>> reply)
        {
            Func<string, bool> to;
            lock (sending) to = send;
            if (to == null) return false;
            var header = Header("commandRequest");
            string id = (string)header["requestId"];
            lock (waiting) { if (waiting.Count > 20) waiting.Clear(); waiting[id] = reply; }
            var request = new System.Collections.Generic.Dictionary<string, object>
            {
                { "header", header },
                { "body", new System.Collections.Generic.Dictionary<string, object> { { "version", 1 }, { "commandLine", line }, { "origin", new System.Collections.Generic.Dictionary<string, object> { { "type", "player" } } } } }
            };
            if (to(Json.Serialize(request))) return true;
            lock (waiting) waiting.Remove(id);
            return false;
        }

        // The feed: one "/capri feed" in flight at a time, about once a second. Without the add-on (or an older one) the
        // answer isn't a feed, so after a few misses she only tries once a minute.
        private void Poll(int mine)
        {
            int misses = 0;
            DateTime asked = DateTime.MinValue, pausedUntil = DateTime.MinValue;
            bool flying = false;
            while (mine == run)
            {
                Thread.Sleep(1200);
                if (send == null) { misses = 0; flying = false; continue; }
                DateTime now = DateTime.UtcNow;
                if (now < pausedUntil || (flying && (now - asked).TotalSeconds < 5)) continue;
                flying = true;
                asked = now;
                bool sent = Ask("capri feed", delegate(System.Collections.Generic.Dictionary<string, object> body)
                {
                    flying = false;
                    var feed = ParseFeed(body);
                    if (feed == null) { if (++misses >= 5) { misses = 0; pausedUntil = DateTime.UtcNow.AddSeconds(60); } return; }
                    misses = 0;
                    lastFeed = DateTime.UtcNow;
                    var heard = Feed;
                    if (heard != null) heard(feed);
                });
                if (!sent) flying = false;
            }
        }

        private static System.Collections.Generic.Dictionary<string, object> ParseFeed(System.Collections.Generic.Dictionary<string, object> body)
        {
            string text = Text(body, "statusMessage") ?? Text(body, "message") ?? "";
            int start = text.IndexOf("{\"v\":", StringComparison.Ordinal);
            if (start < 0) return null;
            try
            {
                var feed = Json.Deserialize<System.Collections.Generic.Dictionary<string, object>>(text.Substring(start));
                return feed != null && feed.ContainsKey("s") ? feed : null;
            }
            catch (Exception) { return null; }
        }

        private static System.Collections.Generic.Dictionary<string, object> Header(string purpose)
        {
            return new System.Collections.Generic.Dictionary<string, object>
            {
                { "version", 1 }, { "requestId", Guid.NewGuid().ToString() }, { "messageType", "commandRequest" }, { "messagePurpose", purpose }
            };
        }

        private void Subscribe(Func<string, bool> to)
        {
            foreach (string name in Events)
            {
                var request = new System.Collections.Generic.Dictionary<string, object> { { "header", Header("subscribe") }, { "body", new System.Collections.Generic.Dictionary<string, object> { { "eventName", name } } } };
                to(Json.Serialize(request));
            }
        }

        // ---------------------------------------------------------------- what comes from Minecraft
        internal void Received(string message)
        {
            System.Collections.Generic.Dictionary<string, object> packet;
            try { packet = Json.Deserialize<System.Collections.Generic.Dictionary<string, object>>(message); }
            catch (Exception) { return; }
            var header = Field(packet, "header");
            var body = Field(packet, "body");
            if (header != null && body != null && Text(header, "messagePurpose") == "commandResponse")
            {
                string id = Text(header, "requestId") ?? "";
                Action<System.Collections.Generic.Dictionary<string, object>> reply;
                lock (waiting) { if (waiting.TryGetValue(id, out reply)) waiting.Remove(id); }
                if (reply != null) reply(body);
                return;
            }
            if (header == null || body == null || Text(header, "messagePurpose") != "event") return;
            string name = Text(header, "eventName") ?? Text(body, "eventName");
            if (name == "PlayerMessage")
            {
                string type = Text(body, "type") ?? "chat";
                string sender = Text(body, "sender") ?? "";
                string said = Text(body, "message") ?? "";
                // Her own tellraw lines, other commands' output and server messages aren't someone talking to her.
                if (type != "chat" || sender.Length == 0 || sender == "External" || said.StartsWith("§d<Capri>", StringComparison.Ordinal)) return;
                Action<string, string> heard = Chat;
                if (heard != null) heard(sender, said);
                return;
            }
            Action<string, System.Collections.Generic.Dictionary<string, object>> game = Game;
            if (name != null && game != null) game(name, body);
        }

        private static System.Collections.Generic.Dictionary<string, object> Field(System.Collections.Generic.Dictionary<string, object> from, string key)
        {
            object value;
            return from != null && from.TryGetValue(key, out value) ? value as System.Collections.Generic.Dictionary<string, object> : null;
        }

        internal static string Text(System.Collections.Generic.Dictionary<string, object> from, string key)
        {
            object value;
            return from != null && from.TryGetValue(key, out value) && value != null ? Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) : null;
        }

        // ---------------------------------------------------------------- hosting: Minecraft connects to her
        private void Host(int mine, int port)
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
            }
            catch (SocketException) { Report("Port " + port + " is busy. Pick another one, like " + (port + 1) + "."); return; }
            try
            {
                Report("Waiting for Minecraft. In game, type: /connect localhost:" + port);
                while (mine == run)
                {
                    if (!listener.Pending()) { Thread.Sleep(200); continue; }
                    TcpClient client = listener.AcceptTcpClient();
                    // One game at a time: a new /connect replaces the old one.
                    Thread serve = new Thread(delegate() { Serve(mine, client, port); });
                    serve.IsBackground = true;
                    serve.Name = "Minecraft game";
                    serve.Start();
                }
            }
            finally { listener.Stop(); }
        }

        private void Serve(int mine, TcpClient client, int port)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                if (!Handshake(stream)) return;
                int game = ++hosted;
                Func<string, bool> to = delegate(string text)
                {
                    try { lock (stream) Frame(stream, 1, Encoding.UTF8.GetBytes(text)); return true; }
                    catch (Exception) { return false; }
                };
                lock (sending) send = to;
                Report("Connected to Minecraft. She's in your chat now.");
                Subscribe(to);
                try
                {
                    string text;
                    while (mine == run && game == hosted && (text = ReadMessage(stream)) != null) Received(text);
                }
                catch (Exception) { }
                lock (sending) if (send == to) send = null;
                if (mine == run && game == hosted) Report("Minecraft left. Type /connect localhost:" + port + " to bring her back.");
            }
        }

        private volatile int hosted;

        private static bool Handshake(NetworkStream stream)
        {
            StringBuilder request = new StringBuilder();
            byte[] one = new byte[1];
            while (!request.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
            {
                if (stream.Read(one, 0, 1) <= 0 || request.Length > 8192) return false;
                request.Append((char)one[0]);
            }
            Match key = Regex.Match(request.ToString(), @"Sec-WebSocket-Key:\s*(\S+)", RegexOptions.IgnoreCase);
            if (!key.Success) return false;
            string accept;
            using (SHA1 sha = SHA1.Create()) accept = Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(key.Groups[1].Value + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
            byte[] reply = Encoding.ASCII.GetBytes("HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: " + accept + "\r\n\r\n");
            stream.Write(reply, 0, reply.Length);
            return true;
        }

        private static void Frame(Stream stream, int opcode, byte[] payload)
        {
            MemoryStream frame = new MemoryStream();
            frame.WriteByte((byte)(0x80 | opcode));
            if (payload.Length < 126) frame.WriteByte((byte)payload.Length);
            else if (payload.Length <= 0xFFFF) { frame.WriteByte(126); frame.WriteByte((byte)(payload.Length >> 8)); frame.WriteByte((byte)payload.Length); }
            else { frame.WriteByte(127); for (int i = 7; i >= 0; i--) frame.WriteByte((byte)((long)payload.Length >> (8 * i))); }
            frame.Write(payload, 0, payload.Length);
            byte[] all = frame.ToArray();
            stream.Write(all, 0, all.Length);
        }

        private static void Fill(Stream stream, byte[] into)
        {
            int got = 0;
            while (got < into.Length)
            {
                int n = stream.Read(into, got, into.Length - got);
                if (n <= 0) throw new EndOfStreamException();
                got += n;
            }
        }

        // One whole text message, joining fragments, answering pings. Null when the game closes the link.
        internal static string ReadMessage(Stream stream)
        {
            MemoryStream message = new MemoryStream();
            while (true)
            {
                byte[] head = new byte[2];
                Fill(stream, head);
                bool last = (head[0] & 0x80) != 0;
                int opcode = head[0] & 0x0F;
                long length = head[1] & 0x7F;
                if (length == 126) { byte[] ext = new byte[2]; Fill(stream, ext); length = (ext[0] << 8) | ext[1]; }
                else if (length == 127) { byte[] ext = new byte[8]; Fill(stream, ext); length = 0; for (int i = 0; i < 8; i++) length = (length << 8) | ext[i]; }
                if (length > 16 * 1024 * 1024) return null;
                byte[] mask = null;
                if ((head[1] & 0x80) != 0) { mask = new byte[4]; Fill(stream, mask); }
                byte[] payload = new byte[length];
                Fill(stream, payload);
                if (mask != null) for (int i = 0; i < payload.Length; i++) payload[i] ^= mask[i % 4];
                if (opcode == 8) { try { lock (stream) Frame(stream, 8, payload); } catch (Exception) { } return null; }
                if (opcode == 9) { lock (stream) Frame(stream, 10, payload); continue; }
                if (opcode == 10) continue;
                message.Write(payload, 0, payload.Length);
                if (last) return Encoding.UTF8.GetString(message.ToArray());
            }
        }

        // ---------------------------------------------------------------- connecting out: their own websocket
        private void ConnectOut(int mine, string address)
        {
            int wait = 2000;
            while (mine == run)
            {
                Report("Connecting to " + address + "...");
                using (ClientWebSocket socket = new ClientWebSocket())
                {
                    try
                    {
                        socket.ConnectAsync(new Uri(address), CancellationToken.None).Wait(15000);
                        if (socket.State != WebSocketState.Open) throw new IOException("no answer");
                        wait = 2000;
                        object writing = new object();
                        Func<string, bool> to = delegate(string text)
                        {
                            try
                            {
                                lock (writing) socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)), WebSocketMessageType.Text, true, CancellationToken.None).Wait(10000);
                                return true;
                            }
                            catch (Exception) { return false; }
                        };
                        lock (sending) send = to;
                        Report("Connected to " + address + ".");
                        Subscribe(to);
                        byte[] buffer = new byte[16384];
                        MemoryStream message = new MemoryStream();
                        while (mine == run && socket.State == WebSocketState.Open)
                        {
                            WebSocketReceiveResult got = socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
                            if (got.MessageType == WebSocketMessageType.Close) break;
                            message.Write(buffer, 0, got.Count);
                            if (!got.EndOfMessage) continue;
                            Received(Encoding.UTF8.GetString(message.ToArray()));
                            message.SetLength(0);
                        }
                        lock (sending) if (send == to) send = null;
                    }
                    catch (Exception error)
                    {
                        Exception inner = error is AggregateException && error.InnerException != null ? error.InnerException : error;
                        if (mine == run) Report("Couldn't reach " + address + ": " + Voice.Short(inner.Message) + " Trying again...");
                    }
                }
                if (mine != run) return;
                Thread.Sleep(wait);
                wait = Math.Min(60000, wait * 2);
            }
        }
    }
}
