# Fall, Damn You!

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod that makes an open door under a critter count as open air.

## The problem

In the unmodded game a critter standing on a door keeps standing on it when the door opens, and critters walk across open doors as if they were floor. Only a critter that is already falling passes through an open door. The reason is in how doors are built: a door marks its top row of cells as floor for its whole life, open or closed, and every creature's navigation table treats such a cell as ground.

## What the mod does

For critters, a door they could walk through, meaning one that is open to critters and not solid, no longer counts as floor. The navigation table marks the cell above it as not walkable, the critter's own fall check finds no floor there and nothing solid under its feet, and it falls through, exactly as if the tile had been dug out. In practice:

- A critter standing on a door falls through the moment the door is set to open, by hand or by automation.
- Critters do not path across open doors any more.
- A closed or locked door still carries critters, and so does an automatic door that merely opened to let a duplicant through, because critters cannot pass those.
- Duplicants and robots are not affected: their navigation uses a separate flag the mod leaves alone.

## No waiting

Whether a critter should fall is something its brain checks on the game's brain schedule, so even in the unmodded game a critter can stand on nothing for a moment, for example in a standard pez dropper, where a pneumatic door closes on a critter standing on another pneumatic door. The mod listens for the navigation update that follows a door change and, when a critter is standing in a cell a door just took the floor from, either by opening under it or by closing on it, has that critter's brain run its check right away. The brain decides as it always does; the mod only stops it waiting its turn. The listener costs nothing unless a door changed state within the last second, so the constant navigation updates from digging and building are never inspected.

The mod is one patch on the game's floor validator plus that listener; it does not touch doors, critters or the sim. It has been checked against Fast Track, which keeps both hooks intact.

## Options

Both features are options, on by default, and a change applies when a game is loaded:

- **Open doors are open air to critters** is the floor change.
- **Start falling at once** makes a critter fall the moment a door takes its floor away. With it off, the critter still falls, once it figures out it's supposed to, as in a standard pez dropper.

## Installing

As a local mod:

1. Download `FallDamnYou-<version>.zip` from the [latest release](https://github.com/isochronous/fall-damn-you/releases/latest).
2. Extract it into a new folder named `FallDamnYou` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\FallDamnYou`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/FallDamnYou`
3. Enable it in the game's Mods menu and restart.

## Building

```
git clone --recurse-submodules https://github.com/isochronous/fall-damn-you.git
dotnet build fall-damn-you/src/FallDamnYou -c Release
```
