# NOXMFD Extension: Remote Control Missile Camera POC

This is a **disposable proof of concept**, not a maintained mod. It exists to demonstrate how a
third-party BepInEx plugin can add a page to [NOXMFD](https://github.com/roke77/NOXMFD) — the
"MissileCamera: Remote Control" MFD page — entirely through NOXMFD's public extension API,
without touching NOXMFD's own source at all.

It's a port of the RC camera work originally prototyped by lupfine, rebuilt against NOXMFD's
extension API (`NOXMFD.Api`) instead of living inside NOXMFD itself.

**This repo will be deleted once its purpose is served.**

## What's here

Mirrors NOXMFD's own `src/plugin` + `src/web` split.

- `src/plugin/Plugin.cs`, `RcLifecycle.cs`, `RcCommands.cs`, `RcTelemetry.cs`, `RcAssets.cs` — the plugin.
- `src/plugin/McBridge.cs`, `RcBridge.cs`, `RcFeed.cs` — reflection-based soft dependencies on other
  mods (Mirror Camera, RC-style controls) that don't know this plugin exists.
- `src/web/rc.html`, `rc.css`, `rc.js` — the MFD page itself, served by NOXMFD and standardized on the
  extension telemetry contract (`{type:'ext', data}`), sent to `/ext/rc-missile-camera/command`.
- `lib/NOXMFD.dll` — a prebuilt copy of NOXMFD, referenced only so this project compiles standalone.
  It is **not** shipped or loaded twice — see the build/install notes below.

## Building

Requires the .NET SDK and a Nuclear Option install with BepInEx 5 + NOXMFD already installed.

```
dotnet build -c Release
```

If your game isn't at the default Steam path (`C:\Program Files (x86)\Steam\steamapps\common\Nuclear Option`),
create a local `GameDir.props` next to `RcMissileCamera.csproj` (gitignored, machine-specific):

```xml
<Project><PropertyGroup>
  <GameDir>D:\SteamLibrary\steamapps\common\Nuclear Option</GameDir>
</PropertyGroup></Project>
```

The build copies `RcMissileCamera.dll` straight into `<GameDir>\BepInEx\plugins`.

## Installing

1. Install NOXMFD itself first (this plugin declares a hard `[BepInDependency]` on it and won't
   load without it).
2. Drop `RcMissileCamera.dll` into `BepInEx/plugins/`. Do **not** also copy `lib/NOXMFD.dll` there
   — that's the same assembly NOXMFD's own plugin already loads; a second copy would double-load it.
3. Launch the game. The RC CAM page appears under NOXMFD's EXT nav automatically — no NOXMFD
   changes needed.

## Updating `lib/NOXMFD.dll`

`lib/NOXMFD.dll` is just a build-time reference so this project compiles without NOXMFD's source
tree present. If NOXMFD's `Api` surface changes, rebuild NOXMFD and replace `lib/NOXMFD.dll` with
the new one.
