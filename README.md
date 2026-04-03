# CEImprovements

A [MelonLoader](https://melonwiki.xyz/) mod for **Flyout** that improves the Craft Editor with better camera controls.

## Features

- **WASD movement** with smooth acceleration
- **FPS-style camera** — Shift+mouse for free look, WASD to fly around
- **E/Q** for vertical movement (up/down)
- **Shift** to move faster
- **Smooth zoom** that bypasses the game's zoom clamp (scroll wheel)
- **FOV adjustment** with +/- keys, numpad +/-, or Ctrl+scroll
- **Vertical rotation clamping** — prevents flipping over the top/bottom
- **Auto-focus on selected part** — toggle with F6
- **Toggle mod on/off** with F8 for when you want vanilla controls
- **CSE-safe** — all mod controls automatically disable in the Cross Section Editor

## Install

1. Install [MelonLoader](https://melonwiki.xyz/) for Flyout
2. Drop `CEImprovements.dll` into your Flyout `Mods/` folder

## Building from source

1. Set your game path:
   ```bash
   export FLYOUT_DIR="/path/to/Steam/steamapps/common/Flyout"
   ```
   Or pass it directly: `dotnet build -c Release -p:GameDir="/path/to/Flyout"`
2. Build with `dotnet build -c Release`

## Controls

| Key | Action |
|-----|--------|
| W/A/S/D | Move camera |
| E/Q | Move up/down |
| Shift | Speed multiplier |
| Shift + Mouse | FPS free look |
| Scroll | Zoom in/out |
| +/- | Adjust FOV |
| Ctrl + Scroll | Adjust FOV |
| F6 | Toggle auto-focus on selected part |
| F8 | Toggle mod on/off |

## Project Structure

```
CEImprovements/
  src/
    CEImprovementsMod.cs       Main mod entry point
    FpsCameraController.cs     WASD movement and FPS mode
    ZoomController.cs          Smooth zoom with harmony override
    FovController.cs           FOV adjustment
    AutoFocusController.cs     Auto-focus on selected part
    RotationClamper.cs         Vertical rotation limits
    InputHelper.cs             Input field focus detection
    Patches/
      CECameraZoomPatch.cs     Harmony postfix to bypass zoom clamp
      CSEOpenPatch.cs          Tracks cross section editor state
```

## License

[MIT](LICENSE)
