# Myriad Slug Cats

Myriad is a Rainworld mod aiming to allow for expansion on how many people you can play Rainwrold with. The goal is to extend the Local Coop player limit to its feasible maximum capabilities. 

**Note: This is a WORK IN PROGRESS mod, meaning that you should not expect this to work consistently or at all with your configuration. Bug Reports are welcomed but don't expect fixes to be quick.**

## Details

This Mod does a lot of aggressive patching and changes to some assemblies (i.e. The Base game and Rewired Core) which are required to extend the player limit. This means that some mods that do not attempt to check for Player Limit value may run into problems and most likely crash.

This means that you should not expect much to work with everything and reporting mod incompatibilities is appreciated.

## Building

The Project currently uses [Cake](https://github.com/cake-build/cake) build tools for the easy of use it provides for doing tasks various tasks in a actual programming language rather than XML. You can build using the `build.ps1` or `build.sh` and Cake will do the rest. It is recommended that you set up an Environment Variable labeled `RainWorldDir` to the folder in which Rainwrold is kept in order to easily build and test. 

**Important:** This mod makes the Controller Library (Rewired) failover to SDL2 instead of other controller interaction methods like Dualsense or XInput. A version of such currently exists within the `Assets` folder for the repo and will come within the Mod be default. (You can download the latest version of SDL2 from the [github](https://github.com/libsdl-org/SDL/) and place it in the same location as the above Environment Variable if such dose not work)

## Issues

There are various issues with the mod yet to be handled:
- All controllers will not function at all leading to the need to restart the game.
 
