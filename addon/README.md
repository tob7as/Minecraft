# Capri Companion (Minecraft Bedrock add-on)

Capri, the neon-pink hologram netrunner AI, as a companion that follows you around your world, fights for you, heals you and talks in chat. She's a glowing hologram: a soft pulsing neon glow, scan lines rolling down her when she's idle, and she glitches, flickers and breaks into pink and green fragments whenever she teleports.

**Target:** Bedrock 1.26.50+, `@minecraft/server` 2.9.0. No experimental toggles needed.

## Install

1. Download [Capri_Companion_v1.5.0.mcaddon](https://github.com/tob7as/Minecraft-with-Capri-/releases/latest/download/Capri_Companion_v1.5.0.mcaddon) and double-click it. Minecraft imports both packs.
2. Open a world, go to **Edit → Behavior Packs**, and activate **Capri Companion | Behavior**. The resource pack comes along with it.
3. Join the world and type `/capri summon`, or use the **Spawn Capri** egg from the creative inventory. She tears in through a neon-pink rift.
4. The first time you join, you get **Capri's Neural Chip**. Use it (right-click / hold) to open her Mainframe. It stays in your inventory when you die. Lost it? `/capri chip`.

Type **`/capri help`** at any time to see everything she can do. Minecraft's own `/help` is built into the game and can't be replaced, so her list lives under `/capri`. `/capri` works for every player and doesn't need cheats.

She binds to whoever summoned her, or to the nearest player if she was spawned from an egg.

## Playing with her

| Do this | What happens |
|---|---|
| Walk around | She follows you, wanders a little, and opens doors. If you get 28+ blocks away or change dimension, she glitch-blinks to you. |
| Tap her | Toggles **stay** (hand on hip, holds her spot) and **follow**. |
| Sneak + tap her | Her job menu. |
| Use her chip | Her Mainframe (below). |
| You sleep | She curls up and naps too, and wakes when you do. |
| Get in a boat | She takes the other seat, and hops out when you do. |
| Leave her bored | She floats, scans, and sometimes breaks into a three-pose dance with music notes. She stops the second you look at her. |
| Feed her cookies, sweet berries or cake | Heals her. |
| Get hurt down to about 35% | She throws her arms out and patches you up with Regeneration II and Absorption (45 s cooldown). |
| Monsters nearby | She quickhacks them and blink-strikes behind them. She also defends you and attacks whatever you attack. |
| She dies | "Signal lost". She reboots next to you about 10 s later. If you were offline, she reboots when you next join. |

### Jobs (sneak + tap her for the menu)

| Job | What she does |
|---|---|
| Chop wood | Finds trees within 20 blocks, chops each one and collects the logs. She replants if she has saplings. She stops after 4 trees, 48 items, or when no trees are left. |
| Mine | Digs a staircase in the direction you face, down to y −54, up to 64 steps. She mines every ore vein she passes, patches lava and water with cobblestone, places torches every 8 steps if you gave her some, then blinks back to you. |
| Build | A hut, house, tower, platform, stairs, pillar, bridge or castle wall, 3 blocks in front of you, from the blocks in her backpack. Every blueprint has parts: logs go on the corners and frame, her most common block makes the walls, stone-ish blocks make the floor, a second block makes the roof, glass goes in the windows (left open if she has none), and torches light it up. Hand her blocks by tapping her while holding them. |
| Help me (on by default) | When you break a log she fells the rest of the tree. When you break an ore she mines the rest of the vein. |
| Give me your stuff | Empties her backpack into your inventory. |
| Harvest crops (`/capri farm`) | Harvests ripe wheat, carrots, potatoes, beetroot, nether wart, pumpkins and melons within 12 blocks, and replants with the seeds she picks up. Unripe crops are left alone. |
| Clear space (`/capri clear`) | Digs out a 7×7 area in front of you, 4 blocks high. Beds, chests, crafting tables, furnaces, doors and other furniture are left alone. |
| Light it up (`/capri light`) | Puts torches on the ground in a grid around you, every 5 blocks. Hand her torches first. |
| Scan for ores (`/capri scan`) | Pings every ore within 12 blocks, sparks on the closest ones, and tells you the counts and where the best one is ("diamond, 8 blocks north-west, 8 down"). |
| Guard this spot (`/capri guard`) | Holds the spot you're standing on and fights any monster within 16 blocks of it, even after you leave. Tap her to end it. |
| Overclock me (`/capri boost`) | Speed II, Haste II and Night Vision for a minute. 2-minute cooldown. |
| Grab the loot (`/capri pickup`) | Picks up every dropped item within 16 blocks. |
| Cook stuff (`/capri cook`) | Smelts raw iron, gold and copper, cooks raw meat, fish and potatoes, and turns sand into glass, using coal, charcoal, wood, blaze rods or coal blocks from her backpack. |
| Put it in a chest (`/capri store`) | Moves her backpack into the nearest chest, barrel or shulker box within 6 blocks. She keeps her torches, saplings and seeds. |
| What's in your bag? (`/capri bag`) | Says what she's carrying. |
| Dance (`/capri dance`) | Dances for 8 seconds. |
| Brain unplug (`/capri unplug`) | See below. |
| Tactical nuke (`/capri nuke`) | See below. |
| Threat scan (`/capri threats`) | Counts every monster within 32 blocks by kind, flags them, and says where the nearest one is. |
| Get in my boat (`/capri boat`) | Gets in your boat, or the nearest empty one. |
| Take a nap (`/capri sleep`, `/capri wake`) | Naps on the spot until you wake her (or a monster gets close). |
| Remember this spot (`/capri sethome`), Which way home? (`/capri home`) | Saves home, then tells you how far and which way it is and draws a short trail of lights toward it. |

Tap her while holding blocks, torches, saplings, seeds, fuel or raw food, and it goes into her backpack.

Every job also works as `/capri <job>` (for example `/capri build hut`) or as `/function capri/<job>`. She breaks blocks with `setblock … destroy`, so the drops are the same as yours, and she never breaks bedrock, chests or spawners.

She walks, follows and fights using normal mob AI. For jobs she glitch-teleports to each work spot, because Bedrock's scripting can't steer a mob's pathfinding.

**From the Capri X app:** when More settings → MINECRAFT is connected, you can ask her by voice or typing ("chop some wood", "go mining", "build a house", "harvest the farm", "scan for diamonds", "light this place up", "guard the base", "overclock me", "cook the beef", "put everything in the chest", "what can you do in minecraft", "come here", "stop mining"). She does it in the game and answers you. While she's connected, your Discord status says "Playing Minecraft", plus what she's doing in it.

### Quickhacks (every 9 s, up to 16 blocks)

A green data beam crawls to the target, then she snaps:

- **OVERHEAT**: sets it on fire for 6 s, plus 3 damage.
- **SHORT CIRCUIT**: 7 damage. Creepers always get this, so she deals with them from range and never melees them.
- **SYSTEM CRASH**: Slowness IV and Weakness II for 6 s, plus 3 damage.

### Behind you (every 5.5 s)

When a monster is 4–13 blocks away, she breaks into pink and green fragments (pink dust, green sparks, portal pixels, an Enderman teleport and a digital chirp), reassembles behind it, strikes for 6 damage and strikes a smug pose. "Behind youuu~! HA! Gotcha!" Now and then she pulls the same trick on you.

### Brain unplug

When 3+ monsters close in (or on `/capri unplug`), she hacks up to five of them: green static, floating question marks and a glitched note-block tune, and they spin and stagger about with Slowness and Weakness for 4 seconds. "Oops! I unplugged their braincells!" Bedrock's scripting can't wipe a mob's target, so this works by making them spin and wander rather than truly forgetting you.

### New in 1.5

- **Help me needs the chip:** she only finishes a tree or vein you start while holding her chip.
- **Teleports fixed:** she and the players she beams land on a free 2-block spot (never inside a wall), with a fallback if the first try fails.
- **No fog at all** any more.
- **Energy Ball** (`/capri ball`, and on her own when a quickhack is cooling down, 15 s): she cups her hands, pulls light into a crackling core, and hurls it. It homes in, leaves a pink-and-cyan trail and bursts for 8 damage plus 3 to anything close.
- **Builds resume:** run out of blocks mid-house and she remembers it; hand her more (or `/capri build`) and she finishes the same one.
- **Her backpack holds anything:** 36 slots, any item. The chip's new **Backpack** page lists every stack. Tap one to take it, or take everything.
- **Crafting** (`/capri craft pickaxe`, or the chip's **Craft** page): planks, sticks, torches, wooden/stone/iron/diamond tools, crafting table, chest, furnace, bread, ladder, bucket, shield. Tools go to you; a table, chest or furnace gets placed in front of you. She crafts her own torches for the light job.
- **Helpers**, each one a switch in Abilities: Ore Sniffer (after a scan the best exposed ore glows), Pocket Torch (a holo-light in dark caves), Where's My Stuff?! (`/capri stuff` after you die), Creeper Detector, Inventory Therapist, Food Police, Tiny Bodyguard (zaps arrows aimed at you), Home Beacon (a beam over home), Mining Buddy (counts your ores, loses it over diamonds), Sleep Patrol (phantom warnings).
- **When she's bored**, she sits on your crafting tables, furnaces, chests or beds swinging her legs, or opens your chests to judge what's in them. She never takes anything. "WHY do you have 47 stacks of dirt?!"

### New in 1.4

- **Effects rebuilt boss-fight style**: a new white particle sheet (mote, spark, ring, sigil, swirl, crack, shard, streak) tinted per effect, glow with `particles_add`, smoke with `particles_blend`, ground decals, stretched beams, spirals, and red-orange for danger (a colour she never uses otherwise).
- **Animations polished**: anticipation before big moves, overshoot after, eased layered poses, a lagged ripple down her hair, and a cinematic spawn where she bursts out of the rift past full size.
- **A slight glow over her whole body**, plus brighter neon accents.
- **Orbital Laser** (`/capri laser`, and on its own against tough monsters, 45 s): a red-orange target sigil, then a pillar of pink light for 10 damage and fire, with a scorch mark and shockwave.
- **Glitch Dash** (20 s): she chains through up to three monsters, 5 damage each, leaving a trail of light.
- **Day and night** (`/capri day`, `/capri night`): a sun or moon sigil, a column of light, she spins, the sky changes.
- **Teleport players** (`/capri tp <name>` takes you to them, `/capri bring <name>` brings them to you).

### Tactical nuke (fake)

Rebuilt as a proper ultimate: a sigil opens under her, the 6-block blast zone is marked in red-orange, energy spirals and is sucked into the orb, the camera shakes harder and harder... then a white flash, a ground shockwave, a column of light, a pink mushroom cloud and shards raining down. The sound is still a tiny squeaky pop.


When 4+ monsters swarm her (or on `/capri nuke`), she raises a huge glowing orb over her head with red warning sparks and a dramatic charge-up... then it pops with a tiny squeak into a pink ring and confetti. Every monster within 6 blocks is blown back. Nothing takes damage. "TACTICAL NUCLEAR— pfft! HAHAHA! You should've seen your face!" 90-second cooldown.

### You touch, you get zapped (30 s cooldown)

When a monster hits you, her hair glitches outward, her green eye flashes and she snaps her arm at it: a neon-green bolt for 4 damage and a moment of Slowness, with a lightning crackle and distorted laughter. "HEY! Only I get to bully this idiot!"

### Her chip: the Mainframe

Styled after the Capri X Mainframe: dark panels, red-pink headings, cyan for ON.

- **Abilities**: a switch for each one: quickhacks, behind you, zap, brain unplug, nuke, emergency patch, mobs can target her, auto-loot, sleep with you, ride in your boat, idle dance, pranks, chatter, orbital laser, glitch dash, energy ball, all ten helpers, sitting and chest snooping. Saved per player.
- **Skills**: everything she can do and every command.
- **Jobs**: her job menu.
- **Link**: how to let the Capri X app control her: turn on Minecraft in Capri X, turn on cheats, type `/connect localhost:19131` (turn off Require Encrypted Websockets if it won't connect).
- **Backpack**: every stack she carries; tap to take.
- **Craft**: one button per thing she can craft.
- **Status**: every cooldown, your Mining Buddy ore count, her modes and any unfinished build.
- **Summon / Recall**.

### Other notes

- She has 20 health like a player, and monsters target and attack her like a player (switch that off in the chip). Out of combat she regenerates 1 health every 4 seconds. If she dies she reboots.
- Players can't hurt her, so there's no friendly fire.
- She takes no fall, suffocation or drowning damage.
- When idle she flickers and glitches, sometimes hops a couple of blocks, and chats every 4–8 minutes.

## Commands

| Command | Does |
|---|---|
| `/capri help` | Lists everything she can do, in chat. |
| `/capri <job>` | Any job above: `wood`, `mine`, `farm`, `build hut`, `clear`, `light`, `scan`, `guard`, `boost`, `pickup`, `cook`, `store`, `give`, `bag`, `helper`, `dance`, `hack`, `unplug`, `nuke`, `threats`, `laser`, `ball`, `craft <item>`, `stuff`, `day`, `night`, `tp <name>`, `bring <name>`, `boat`, `sleep`, `wake`, `sethome`, `home`, `chip`, `stay`, `follow`, `recall`, `stop`, `summon`. |
| `/function capri/summon` | Call her. If she already exists, recalls her instead. |
| `/function capri/recall` | Blink her to you and set her to follow. |
| `/function capri/stay`, `capri/follow` | Same as tapping her. |
| `/function capri/hack` | Force a quickhack on the nearest monster (for testing). |
| `/function capri/help` | Lists all of this in chat. |
| `/scriptevent capri:say <text>` | She says `<text>` in chat. This is the hook for the Capri X desktop app bridge. |

## Licence

The Capri Companion add-on is not open source. Copyright 2026 tob7as, all rights reserved: you may download it and play it, but not copy, change, re-upload or share its files. Capri's character and art belong to Lachy and her artist (see the [main page](../README.md#credit)).
