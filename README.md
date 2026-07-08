# Fusion Protector

## About

Fusion Protector is a protection/mitigation system I made for Fusion. The goal was never to make something flashy—it was simply to reduce abusive client behavior where possible and fix problems that negatively affected normal players.

This project was worked on over the course of about six months. It wasn't six months of nonstop work, but something I kept coming back to whenever I found a bug, an exploit, or had a new idea worth implementing.

---

## Why I made this

The idea behind Fusion Protector has always been pretty simple.

- Make abusing the client harder.
- Patch or mitigate known exploits.
- Improve the experience for legitimate players.

That's really all there is to it.

---

## About the code

The codebase isn't perfect, and I know that.

This was mostly a solo project, so I was usually more concerned with fixing whatever was broken than making every file look clean. As new exploits appeared, features were added, rewritten, removed, or expanded, and over time the project naturally became what it is now.

Could parts of it be cleaner? Definitely.

But everything here was written with solving problems first in mind.

---

## AI

Since people tend to ask...

Yes, I used AI while working on this project.

Mostly for brainstorming ideas, cleaning up code, explaining things, or saving time on repetitive tasks. I wasn't blindly copying code I didn't understand. If something ended up in the project, I made sure I knew what it was doing first.

To me it was just another development tool, no different than using documentation or Stack Overflow.

---

# Features

This isn't everything that's included, just the more noticeable parts.

## Client Protection

- Malicious client detection
- Various exploit mitigations

## Management

- Debug UI (Press **Y**)
- Permission-based menus
- Recently Met Players list
- Quick player moderation
- Open Steam profiles directly

## Search Tools

- Spawnable Searcher
- Avatar Searcher
- Level Searcher

## Customization

- Developer presets
- Custom Bodylog pages
- Bodylog color presets
- Radial Menu color presets

## Loadouts

- Save inventory presets
- Instantly apply saved loadouts
- Keep Inventory between lobbies

## Emergency Controls

- Emergency Escape (**Y + B**)
- Emergency Avatar Escape (**X + A**)
- Emergency Scene Reload (Host)
- Full emergency reset by holding both thumbsticks

## Safety

- Spawn blocking
- Warning system
- Avatar kick lists
- Alternate account detection
- NSFW content blocker
- Media protection

## Quality of Life

- Custom Home World selection
- Profile presets
- UI improvements throughout Fusion

## Owner Tools

- NPC spawners
- Map-locked teleporters

## Moderation

- Toggle Fusion's Global Ban List if needed

---

## Security

This project isn't meant to help anyone make a malicious client.

Anything that was exploited while developing Fusion Protector was paired with a mitigation or fix. Randomly copying pieces of this project probably won't be very useful if someone's goal is to abuse the game.

---

## Forking

If you want to fork this project, go for it.

If you find something useful, feel free to reuse it, improve it, or build your own version.

If parts of Fusion Protector eventually make their way into Fusion itself, that would honestly be the best outcome. The whole reason this project exists is to improve the experience for everyone.

The only thing I ask is that you give credit where it's due.

---

## Known Issue

If your game crashes while editing permissions as host, delete the `userdata` folder inside your BONELAB installation.

This only seems to happen in some cases when updating from older versions of Fusion Protector.

---

## Building

If you want, you can have Visual Studio automatically copy the DLL directly into your BONELAB Mods folder after every build.

Example:

```xml
<Target Name="PostBuild2" AfterTargets="PostBuildEvent">
    <Exec Command="copy &quot;$(TargetPath)&quot; &quot;G:\GAMES TO PLAY ON CHANNEL\BONELAB-SteamGG.NET\Mods\&quot;" />
</Target>
```

---

## Thanks

Big thanks to Lakatrazz for making Fusion in the first place.

Thanks as well to everyone who reported bugs, suggested ideas, tested builds, or just gave feedback throughout development. You know who you are.

---

## Final words

I'm releasing this because I've decided it's time to move on.

I enjoyed working on Fusion Protector, learned a lot while making it, and met some genuinely good people because of it.

Hopefully someone finds it useful, learns something from it, or continues improving on the ideas here.
