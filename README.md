# Capri for Minecraft

Two parts:

- **[addon/](addon/)**: the Capri Companion add-on for Minecraft Bedrock (1.26.50+). Grab `addon/dist/Capri_Companion_v1.5.0.mcaddon` and double-click it to import. Everything she can do is in [addon/README.md](addon/README.md).
- **[app/](app/)**: Capri X, a fork of the Capri desktop companion with a Minecraft link. Say "capri, chop some wood" and her body in the game does it. Build it with `ci/build-windows.ps1`.

## Setup

### 1. The add-on (needed)

1. Download [`addon/dist/Capri_Companion_v1.5.0.mcaddon`](addon/dist/Capri_Companion_v1.5.0.mcaddon) and double-click it. Minecraft opens and imports both packs.
2. Make or edit a world. Under **Behavior Packs** turn on **Capri Companion**. The resource pack comes with it.
3. Join the world. You get **Capri's chip** in your inventory. Use it to open her Mainframe, then press **Summon**. You can also type `/capri summon`.
4. Type `/capri help` to see everything she can do. Tap her to make her stay or follow. Sneak and tap her for her job menu.

`/capri` works without cheats. Needs Minecraft Bedrock 1.26.50 or newer.

### 2. The Capri X app (optional: talk to her)

With the app, you can speak or type to her on your PC ("capri, chop some wood") and her body in the game does it.

1. Build it on Windows 10 or 11. You need the Windows 10/11 SDK. In PowerShell, from the `app` folder:
   ```powershell
   powershell -ExecutionPolicy Bypass -File ci\build-windows.ps1
   ```
   This makes `out\windows\Capri.exe` and `Capri-Setup.exe`.
2. Run it and pick her brain and voice in the Mainframe. [app/README.md](app/README.md) explains the options, including free ones.
3. In Capri X, open **More settings** and turn on **Minecraft**.
4. In Minecraft, turn on **cheats** for the world, then type this in chat:
   ```
   /connect localhost:19131
   ```
   If it won't connect, go to **Settings → General** in Minecraft and turn off **Require Encrypted Websockets**.
5. Say or type "capri, ..." in the app or in game chat. She answers out loud and in chat.

The chip's **Link** page in game shows these steps too.

## Credit

Capri is made by Lachy: <https://github.com/lachydotmcg/capri-desktop>. Capri's art and animations are by her artist, who prefers to stay private. The app is shared under the original project's PolyForm Noncommercial License 1.0.0 ([app/LICENSE.md](app/LICENSE.md)). Those terms also cover the add-on, since it uses Capri's character.

Required Notice: Copyright 2026 Lachy (https://github.com/lachydotmcg)
