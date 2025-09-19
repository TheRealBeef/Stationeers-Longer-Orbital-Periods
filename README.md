# Beef's Longer Orbital Periods

<p align="center" width="100%">
<img alt="Icon" src="./About/thumb.png" width="45%" />
</p>

This plugin gives you control over the length of the day/night cycle in Stationeers. You can set it really fast, really slow, however you want.

It works as a server-side only mod, not requiring clients to have it installed. The mod automatically disables its patch if it detects you're connecting to someone else's world.

## Requirements

**WARNING:** This is a StationeersLaunchPad Plugin Mod. It requires BepInEx to be installed with the StationeersLaunchPad plugin.

See: [https://github.com/StationeersLaunchPad/StationeersLaunchPad](https://github.com/StationeersLaunchPad/StationeersLaunchPad)

## Installation

1.  Ensure you have BepInEx and StationeersLaunchPad installed.
2.  Install it from the workshop. Alternatively: Place the dll file into your `/BepInEx/plugins/` folder.

## Usage

You can configure the multiplier in the StationeersLaunchPad config, or you can set it in-game using the console. Changes made via the console are automatically saved to the config file and will persist between game sessions.

Press `F3` to open the console and use the `time` command.

### Commands

- To see current settings:
  ```
  time
  ```
- To set a new multiplier:
  ```
  time <multiplier>
  ```

### Examples

* `time 0.5` (A fast 10-minute day)
* `time 1.0` (Default 20-minute day)
* `time 3.0` (A 1-hour day - mod default)
* `time 6.0` (A 2-hour day)

**Note:** This mod only affects the sun's orbital period and does not change the duration or frequency of storms.

## Changelog

>### Version 2.0.0
>- Complete rewrite to update to current stationeers
>- Added a console command system (`time [multiplier]`).
>- Console command changes are automatically saved to the config file.

## Roadmap

Nothing for now, about as complete as it needs to be

## Source Code

The source code is available on GitHub:
[https://github.com/TheRealBeef/Stationeers-Longer-Orbital-Periods](https://github.com/TheRealBeef/Stationeers-Longer-Orbital-Periods)