# Capri for Minecraft

## ⬇️ Download

<a href="https://github.com/tob7as/Minecraft/raw/main/addon/dist/Capri_Companion_v1.5.0.mcaddon"><img src="images/download-minecraft.png" alt="Download for Minecraft"></a>&nbsp;&nbsp;<a href="https://github.com/tob7as/Minecraft/raw/main/download/Capri-X-Setup.exe"><img src="images/download-windows.png" alt="Download for Windows"></a>

**Minecraft:** the add-on. Double-click it and Minecraft imports it. **Windows:** the Capri X app, so you can talk to her.

| What | Download | |
|---|---|---|
| **Minecraft add-on** | [**Capri_Companion_v1.5.0.mcaddon**](https://github.com/tob7as/Minecraft/raw/main/addon/dist/Capri_Companion_v1.5.0.mcaddon) | Double-click it, Minecraft imports it |
| **Capri X app** (Windows) | [**Capri-X-Setup.exe**](https://github.com/tob7as/Minecraft/raw/main/download/Capri-X-Setup.exe) | Run it to install |
| App, no install | [Capri-X.exe](https://github.com/tob7as/Minecraft/raw/main/download/Capri-X.exe) | Runs on its own |

Click a link and the download starts. Windows may say it "protected your PC" because the app isn't signed: click **More info → Run anyway**. Then follow [Setup](#setup) below.

## See her in action

![Capri in Minecraft: say hello and she answers in a chat bubble, with her body standing beside you](images/capri-in-game.webp)

Click a clip, then **View raw**, to download it with sound.

| | |
|---|---|
| [![Capri dancing](images/dancing.webp)](videos/dancing.mp4)<br>**Dancing:** "drop the beat!!" | [![Capri fighting](images/fighting.webp)](videos/fighting.mp4)<br>**Fighting:** she takes on mobs with you |
| [![Capri at day and night](images/day-and-night.webp)](videos/day-and-night.mp4)<br>**Day and night:** she reacts to sunrise, nightfall and sleep | [![Capri mining](images/mining.webp)](videos/mining.mp4)<br>**Mining:** "digging down for shiny stuff now" |

🎙️ **You can tell her what to do with your voice!** With the Capri X app, just say "Capri, ..." out loud, like "Capri, chop some wood", and she does it in the game.

Two parts:

- **[addon/](addon/)**: the Capri Companion add-on for Minecraft Bedrock (1.26.50+). Grab `addon/dist/Capri_Companion_v1.5.0.mcaddon` and double-click it to import. Everything she can do is in [addon/README.md](addon/README.md).
- **[app/](app/)**: Capri X, a fork of the Capri desktop companion with a Minecraft link. Say "capri, chop some wood" and her body in the game does it. Ready to run from [download/](download/).

## Setup

### 1. The add-on (needed)

1. Download [`Capri_Companion_v1.5.0.mcaddon`](https://github.com/tob7as/Minecraft/raw/main/addon/dist/Capri_Companion_v1.5.0.mcaddon) and double-click it. Minecraft opens and imports both packs.
2. Make or edit a world. Under **Behavior Packs** turn on **Capri Companion**. The resource pack comes with it.
3. Join the world. You get **Capri's chip** in your inventory. Use it to open her Mainframe, then press **Summon**. You can also type `/capri summon`.
4. Type `/capri help` to see everything she can do. Tap her to make her stay or follow. Sneak and tap her for her job menu.

`/capri` works without cheats. Needs Minecraft Bedrock 1.26.50 or newer.

### 2. The Capri X app (optional: talk to her)

With the app, you can speak or type to her on your PC ("capri, chop some wood") and her body in the game does it.

1. Download [`Capri-X-Setup.exe`](https://github.com/tob7as/Minecraft/raw/main/download/Capri-X-Setup.exe) and run it. (Prefer no install? [`Capri-X.exe`](https://github.com/tob7as/Minecraft/raw/main/download/Capri-X.exe) runs on its own.) Windows 10 or 11 only. Windows may say it "protected your PC" because the app isn't signed: click **More info → Run anyway**.
   This version is its own app: it installs to `%LocalAppData%\CapriMC` and keeps its settings, keys and memories there, apart from any other Capri you have.
2. Give her a brain (her AI) in the Mainframe. Both are free:
   - **Free key:** press **Free key** and follow the steps to get a free NVIDIA key. Works on any PC, needs internet.
   - **Set up her brain:** she runs on your own PC, with no key. Needs 8 GB of memory or more, and a 3 to 5 GB download.

   Her free voice works straight away. Other voices and options are in [app/README.md](app/README.md).
3. In Capri X, open the Mainframe's **Settings** page, click **More settings**, and turn on **Play Minecraft with me** in the Minecraft section.
4. In Minecraft, turn on **cheats** for the world, then type this in chat:
   ```
   /connect localhost:19131
   ```
   If it won't connect, go to **Settings → General** in Minecraft and turn off **Require Encrypted Websockets**.
5. Talk to her:
   - **With your voice:** click the **wave** button on her (or press **Ctrl + Alt + V**) to start voice chat, then say "Capri, ..." like "Capri, chop some wood". You need a mic.
   - **By typing:** type "capri, ..." in game chat, or click the **pencil** on her.

   She answers out loud and in chat, and her body in the game does it.

The chip's **Link** page in game shows these steps too.

### The More settings window

<img src="images/more-settings.webp" alt="Capri X's More settings window: Think with Claude, Music, Stream, Vibe, Clip that, Weather, Memory vault and Minecraft" width="400">

Turn on **Play Minecraft with me** at the bottom. The **Memory vault** section is where you pick an Obsidian vault for her notes.

<details><summary>Build the app yourself instead</summary>

You need the Windows 10/11 SDK. In PowerShell, from the `app` folder:

```powershell
powershell -ExecutionPolicy Bypass -File ci\build-windows.ps1
```

This makes `out\windows\Capri.exe` and `Capri-Setup.exe`.
</details>

## Her notes, and Obsidian

Capri keeps her own notes about you: your name, what you like, things you ask her to remember. Tell her "remember that I like..." or press **Get to know me** in the Mainframe. She reads her notes when you chat, and you can open, edit or delete them any time.

- **By default** the notes are a plain file on your PC (`me.md` in her data folder). A new install starts with empty notes.
- **In Obsidian:** in the Mainframe, open **Settings → More settings → Memory vault**, pick one of your Obsidian vaults, and she keeps her notes in a **Capri Memories** note inside it. You can then read and edit what she remembers in Obsidian like any other note.

She has an Obsidian plugin too: **[Capri Neon City](https://github.com/tob7as/obsidian-Plugin)** turns your vault into a holographic 3D city, with Capri as the guide who chats, finds notes and flies you around. It works on its own, no app needed.

![Capri Neon City: an Obsidian vault shown as a 3D neon city, with Capri chatting in the corner](images/capri-neon-city.webp)

## Credit

Capri is made by Lachy: <https://github.com/lachydotmcg/capri-desktop>. Capri's art and animations are by her artist, who prefers to stay private. The app is shared under the original project's PolyForm Noncommercial License 1.0.0 ([app/LICENSE.md](app/LICENSE.md)). Those terms also cover the add-on, since it uses Capri's character.

Required Notice: Copyright 2026 Lachy (https://github.com/lachydotmcg)
