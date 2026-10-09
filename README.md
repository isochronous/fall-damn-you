# Just Fall Already!

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod: a critter whose footing a door just took away falls at once, instead of standing there until it figures out it's supposed to.

## What it does

Whether a critter should fall is something its brain checks when its turn comes up on the game's brain schedule. When a door takes the floor from under a critter, by closing on it in a standard pez dropper, or by opening under it with a mod that makes open doors open air for critters, the critter can stand on nothing for a noticeable moment. This mod watches the navigation update that follows a door finishing its open or close and, for any critter standing in a cell next to a door that its navigation can no longer use, has its brain run its checks right away. The game's own fall check does the rest.

What counts as floor is left to the game and to other mods. [Sgt_Imalas's Critters fall through open doors](https://steamcommunity.com/sharedfiles/filedetails/?id=3816086407) ([source](https://github.com/Sgt-Imalas/Sgt_Imalas-Oni-Mods)) makes open doors non-floor and non-ceiling for critters; with it installed, this mod makes those falls immediate too. Without it, the mod still speeds up pez droppers.

The listener costs nothing unless a door finished a state change within the last second, so the constant navigation updates from digging and building are never inspected. Duplicants and robots are not touched. Checked against Fast Track, which keeps the hooks intact.

## Installing

As a local mod:

1. Download `JustFallAlready-<version>.zip` from the [latest release](https://github.com/isochronous/just-fall-already/releases/latest).
2. Extract it into a new folder named `JustFallAlready` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\JustFallAlready`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/JustFallAlready`
3. Enable it in the game's Mods menu and restart.

## Building

```
git clone --recurse-submodules https://github.com/isochronous/just-fall-already.git
dotnet build just-fall-already/src/JustFallAlready -c Release
```
