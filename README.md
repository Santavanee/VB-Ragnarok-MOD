# UlForce

A [BepInEx](https://github.com/BepInEx/BepInEx) mod for **VenusBlood RAGNAROK International (US)**.

## Features

- **VBR Force Lock** — locks the force division value to 500 (toggle with `F1`), and raises the minimum item limit to 100.
- **VBR Custom Unit** — adds a custom unit, *Faceless Reaper Lulu*, directly to your roster with full stats, 9 skills, and a custom portrait.

## Requirements

- **VenusBlood RAGNAROK International (US)** (Unity 5.6.7, Mono)
- **BepInEx 5.4.23.4** (x86/x64 matching the game's architecture)

## Installation

### 1. Install BepInEx

1. Download **BepInEx 5.4.23.4** (the version matching your game's architecture, typically `x64`) from the [BepInEx releases page](https://github.com/BepInEx/BepInEx/releases).
2. Extract the contents directly into the game's install folder (the folder containing the game's `.exe`, e.g. `...\VenusBlood RAGNAROK International (US)\`). After extracting you should see a `BepInEx` folder, `winhttp.dll`, and `doorstop_config.ini` alongside the game executable.
3. Launch the game once and close it again. This lets BepInEx generate its folder structure, including `BepInEx\plugins\`.

### 2. Get the mod DLL

Either:

- **Download** `UlForce.dll` and `UlForce_lulu.png` from the [Releases](../../releases) page (if available), or
- **Build from source**:
  1. Open `UlForce/UlForce.csproj` and update the `HintPath` entries under `<Reference>` so they point at your own game install's `BepInEx\core\` and `...\Managed\` folders.
  2. Build with MSBuild:
     ```powershell
     & "C:\Program Files\Microsoft Visual Studio\<version>\<edition>\MSBuild\Current\Bin\amd64\MSBuild.exe" `
       "UlForce\UlForce.csproj" /p:Configuration=Debug /nologo /v:minimal
     ```
  3. The compiled DLL will be at `UlForce\bin\Debug\UlForce.dll`.

### 3. Install the mod

Copy the following files into the game's `BepInEx\plugins\` folder:

- `UlForce.dll`
- `UlForce_lulu.png` (rename `UlForce/Assets/lulu.png` to this if building from source — it's the custom portrait loaded at runtime, and the mod won't work correctly without it)

```
VenusBlood RAGNAROK International (US)\
└─ BepInEx\
   └─ plugins\
      ├─ UlForce.dll
      └─ UlForce_lulu.png
```

### 4. Launch the game

Start the game normally. If everything is installed correctly, the Faceless Reaper Lulu unit should already be in your roster, and the force division/item limit tweaks should be active.

To confirm the mod loaded, check `BepInEx\LogOutput.log` for lines from `VBR Force Lock` and `VBR Custom Unit`.

## Notes

- This mod directly patches game data in memory via Harmony; it does not modify any game files.
- See `UlForce/MODDING_NOTES.md` for detailed developer notes on the mod's internals.
