# STS2 Miscellaneous Mods

A Collection of Totally Useless Slay the Spire 2 Mods

## Mods

| Mod | Description |
|-----|-------------|
| [Giantification](gigantification/) | When you have the Gigantification power, your character visually grows |
| [Make Blur Blur](make-blur-blur/) | Make Blur blur |
| [Backflip](sts2-backflip/) | Backflip with Backflip cards |
| [Grand Finale Effect](grand-finale-effect/) | Add effect when use grand finale |

## Build

**Requirements**
- .NET 9.0 SDK
- Godot 4.5.1

Each mod is a separate project. Navigate to the mod folder and run:

```bash
dotnet build
```

If mod have pck file, you can build pck file:

```bash
dotnet publish pck-mod-name-here.csproj
```

When you build pck file, you should specify your Godot Engine path to .csproj file

After building, the DLL is automatically copied to the STS2 mods folder.

> Note: Close the game before building, as the DLL cannot be overwritten while the game is running.
