# NOXMFD Extension: Remote Control Missile Camera

Adds a **MissileCamera: Remote Control** page to [NOXMFD](https://github.com/roke77/NOXMFD)'s MFD
— live seeker feed plus remote-piloting controls (aim, throttle, afterburner, formation follow,
vision mode, manual detonate) for missiles under [MissileCamera Remote
Control](https://github.com/Mursisru/MissileCamera-Remote-Control)'s command. Built entirely
through NOXMFD's public extension API (`NOXMFD.Api`, see NOXMFD's
[`docs/extensions-api.md`](https://github.com/roke77/NOXMFD/blob/main/docs/extensions-api.md)) —
it never touches NOXMFD's own source, and NOXMFD needs no changes to load it.

Requires [MissileCamera](https://github.com/Mursisru/MissileCamera) and [MissileCamera Remote
Control](https://github.com/Mursisru/MissileCamera-Remote-Control) installed alongside it — this
extension is the display/control layer on top of those two mods, not a replacement for either.

**Credit:** the original remote-camera integration — including the `Bridge` API this extension
talks to on the MissileCamera/RC side — was designed and built by
[lupfine](https://github.com/lupfine). This repo is the same feature rebuilt as a standalone
NOXMFD extension, so it ships as its own mod with its own release cycle instead of living inside
NOXMFD's source tree.

## What's here

Mirrors NOXMFD's own `src/plugin` + `src/web` split.

- `src/plugin/Plugin.cs`, `MissileCameraLifecycle.cs`, `MissileCameraCommands.cs`,
  `MissileCameraTelemetry.cs`, `MissileCameraAssets.cs` — the plugin.
- `src/plugin/McBridge.cs`, `RcBridge.cs`, `RcFeed.cs` — reflection-based soft dependencies on
  MissileCamera and MissileCamera Remote Control, which don't know this plugin exists. Kept their
  `Rc`/`Mc` names since those encode which of the two mods each one bridges to.
- `src/web/missile-camera.html`, `missile-camera.css`, `missile-camera.js` — the MFD page itself,
  served by NOXMFD and standardized on the extension telemetry contract (`{type:'ext', data}`),
  sent to `/ext/rc-missile-camera/command`.
- `lib/NOXMFD.dll` — a prebuilt copy of NOXMFD, referenced only so this project compiles
  standalone. It is **not** shipped or loaded twice — see the build/install notes below.

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

The build copies `NOXMFD.RcMissileCamera.dll` straight into `<GameDir>\BepInEx\plugins`.

## Installing

1. Install NOXMFD itself first (this plugin declares a hard `[BepInDependency]` on it and won't
   load without it), plus MissileCamera and MissileCamera Remote Control.
2. Drop `NOXMFD.RcMissileCamera.dll` into `BepInEx/plugins/`. Do **not** also copy `lib/NOXMFD.dll` there
   — that's the same assembly NOXMFD's own plugin already loads; a second copy would double-load it.
3. Launch the game. The RC CAM page appears under NOXMFD's EXT nav automatically — no NOXMFD
   changes needed.

## Updating `lib/NOXMFD.dll`

`lib/NOXMFD.dll` is just a build-time reference so this project compiles without NOXMFD's source
tree present. If NOXMFD's `Api` surface changes, rebuild NOXMFD and replace `lib/NOXMFD.dll` with
the new one.

## Cutting a release

Same practices as NOXMFD itself:

- Tag = **bare semver, no `v` prefix** (`0.1.0`, not `v0.1.0`). The `<Version>` in
  `RcMissileCamera.csproj` and the tag should match.
- Always a **full release** — never `--prerelease`, so it gets GitHub's Latest badge.
- Title is `NOXMFD: RC Missile Camera Extension X.Y.Z`, passed explicitly via `--title` — never
  the bare tag.
- Every release **must have `NOXMFD.RcMissileCamera_X.Y.Z.zip` attached**, containing a single
  `NOXMFD.RcMissileCamera/NOXMFD.RcMissileCamera.dll`. Verify with:
  `gh release view <tag> --json assets -q '.assets[].name'`
- Release notes are a tight changelog of the actual changes only — no install/how-to text (that
  belongs in this README).

```
dotnet build -c Release
# stage NOXMFD.RcMissileCamera.dll under NOXMFD.RcMissileCamera\, zip it as
# NOXMFD.RcMissileCamera_X.Y.Z.zip
gh release create X.Y.Z --title "NOXMFD: RC Missile Camera Extension X.Y.Z" --notes "..." NOXMFD.RcMissileCamera_X.Y.Z.zip
```
