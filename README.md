# STS2 Miscellaneous Mods

A Collection of Totally Useless Slay the Spire 2 Mods

## Mods

| Mod | Description |
|-----|-------------|
| [Giantification](gigantification/) | When you have the Gigantification power, your character visually grows |
| [Make Blur Blur](make-blur-blur/) | Make Blur blur |
| [Backflip](sts2-backflip/) | Backflip with Backflip cards |

## Build

**Requirements**
- .NET 9.0 SDK

Each mod is a separate project. Navigate to the mod folder and run:

```bash
dotnet build
```

After building, the DLL is automatically copied to the STS2 mods folder.

> Note: Close the game before building, as the DLL cannot be overwritten while the game is running.
