# CEImprovements

A [MelonLoader](https://melonwiki.xyz/) mod for **Flyout** that improves the Craft Editor with better camera controls.

## Features

- **WASD movement** with smooth acceleration
- **FPS-style camera** — Shift+mouse for free look, WASD to fly around
- **E/Q** for vertical movement (up/down)
- **Shift** to move faster
- **Smooth zoom** that bypasses the game's zoom clamp (scroll wheel)
- **FOV adjustment** with +/- keys, numpad +/-, or Ctrl+scroll

## Install

1. Install [MelonLoader](https://melonwiki.xyz/) for Flyout
2. Set your game path:
   ```bash
   export FLYOUT_DIR="/path/to/Steam/steamapps/common/Flyout"
   ```
   Or pass it directly: `dotnet build -c Release -p:GameDir="/path/to/Flyout"`
3. Build with `dotnet build -c Release`
4. Copy `bin/Release/net6.0/CEImprovements.dll` to your Flyout `Mods/` folder

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

## Project Structure

```
CEImprovements/
  src/
    CEImprovementsMod.cs       Main mod entry point
    FpsCameraController.cs     WASD movement and FPS mode
    ZoomController.cs          Smooth zoom with harmony override
    FovController.cs           FOV adjustment
    InputHelper.cs             Input field focus detection
    Patches/
      CECameraZoomPatch.cs     Harmony postfix to bypass zoom clamp
```

## License

[MIT](LICENSE)
