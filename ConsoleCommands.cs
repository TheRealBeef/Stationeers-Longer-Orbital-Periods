using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Networking;
using Assets.Scripts;
using Util.Commands;

namespace BeefsLongerOrbitalPeriods
{
    [HarmonyPatch]
    public class ConsoleCommandHandler
    {
        private static readonly Dictionary<string, DayLengthPreset> PresetNames = new Dictionary<string, DayLengthPreset>(StringComparer.OrdinalIgnoreCase)
        {
            { "real-moon",   DayLengthPreset.RealMoon },
            { "real-mars",   DayLengthPreset.RealMars },
            { "real-europa", DayLengthPreset.RealEuropa },
            { "real-venus",  DayLengthPreset.RealVenus },
            { "real-mimas",  DayLengthPreset.RealMimas }
        };

        [HarmonyPatch(typeof(Util.Commands.CommandLine), "Process", new Type[] { typeof(string) })]
        [HarmonyPrefix]
        static bool InterceptConsoleCommands(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return true;
            }

            string[] parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return true;
            }

            string command = parts[0].ToLower();

            if (command == "time")
            {
                if (parts.Length == 1)
                {
                    OrbitalCommands.ShowCurrentSettings();
                    ShowUsage();
                    return false;
                }

                if (parts.Length == 2)
                {
                    string arg = parts[1];

                    // real-world presets
                    if (PresetNames.TryGetValue(arg, out DayLengthPreset preset))
                    {
                        OrbitalCommands.SetPreset(preset);
                        return false;
                    }

                    // otherwise sets to custom mode and parses as number
                    if (float.TryParse(arg, out float multiplier))
                    {
                        OrbitalCommands.SetCustomMultiplier(multiplier);
                        return false;
                    }

                    // Unknown argument
                    ConsoleWindow.Print($"Unknown argument: {arg}", ConsoleColor.Red, false, false);
                    ShowUsage();
                    return false;
                }

                ShowUsage();
                return false;
            }

            if (command == "plants")
            {
                if (parts.Length == 1)
                {
                    OrbitalCommands.ShowPlantSettings();
                    return false;
                }

                if (parts.Length >= 2)
                {
                    string subCommand = parts[1].ToLower();

                    // plants growth ...
                    if (subCommand == "growth")
                    {
                        if (parts.Length == 2)
                        {
                            OrbitalCommands.ShowPlantSettings();
                            return false;
                        }

                        string arg = parts[2].ToLower();

                        // plants growth off/disabled
                        if (arg == "off" || arg == "false" || arg == "disable" || arg == "disabled")
                        {
                            OrbitalCommands.SetPlantGrowthMode(PlantGrowthMode.Disabled);
                            return false;
                        }

                        // plants growth on/daylength
                        if (arg == "on" || arg == "true" || arg == "enable" || arg == "daylength")
                        {
                            OrbitalCommands.SetPlantGrowthMode(PlantGrowthMode.UseDayLength);
                            return false;
                        }

                        // plants growth custom ...
                        if (arg == "custom")
                        {
                            if (parts.Length == 4 && float.TryParse(parts[3], out float customValue))
                            {
                                OrbitalCommands.SetPlantGrowthCustom(customValue);
                            }
                            else
                            {
                                OrbitalCommands.SetPlantGrowthMode(PlantGrowthMode.Custom);
                            }
                            return false;
                        }

                        // plants growth ...
                        if (float.TryParse(arg, out float growthMultiplier))
                        {
                            OrbitalCommands.SetPlantGrowthCustom(growthMultiplier);
                            return false;
                        }
                    }

                    // plants light on/off
                    if (subCommand == "light" && parts.Length == 3)
                    {
                        string arg = parts[2].ToLower();
                        if (arg == "on" || arg == "true" || arg == "enable")
                        {
                            OrbitalCommands.SetPlantLightDarkScaling(true);
                            return false;
                        }
                        if (arg == "off" || arg == "false" || arg == "disable")
                        {
                            OrbitalCommands.SetPlantLightDarkScaling(false);
                            return false;
                        }
                    }
                }

                ConsoleWindow.Print("Usage:", ConsoleColor.Yellow, false, false);
                ConsoleWindow.Print("  plants                  - Show current plant settings", ConsoleColor.Gray, false, false);
                ConsoleWindow.Print("  plants growth off       - Disable growth scaling (vanilla)", ConsoleColor.Gray, false, false);
                ConsoleWindow.Print("  plants growth on        - Scale growth by day length", ConsoleColor.Gray, false, false);
                ConsoleWindow.Print("  plants growth <value>   - Use custom growth multiplier", ConsoleColor.Gray, false, false);
                ConsoleWindow.Print("  plants light on|off     - Toggle light/dark requirement scaling", ConsoleColor.Gray, false, false);
                return false;
            }

            return true;
        }

        private static void ShowUsage()
        {
            ConsoleWindow.Print("", ConsoleColor.White, false, false);
            ConsoleWindow.Print("=== Time Command Usage ===", ConsoleColor.Yellow, false, false);
            ConsoleWindow.Print("time <multiplier>     - Set custom multiplier (e.g., time 6.0)", ConsoleColor.Green, false, false);
            ConsoleWindow.Print("time <preset>         - Use real-world preset", ConsoleColor.Green, false, false);
            ConsoleWindow.Print("", ConsoleColor.White, false, false);
            ConsoleWindow.Print("=== Custom ===", ConsoleColor.Cyan, false, false);
            ConsoleWindow.Print("0.5x = 10min | 1x = 20min | 3x = 1hr (default) | 6x = 2hr", ConsoleColor.Gray, false, false);
            ConsoleWindow.Print("", ConsoleColor.White, false, false);
            ConsoleWindow.Print("=== Presets ===", ConsoleColor.Cyan, false, false);
            ConsoleWindow.Print("time real-moon   - 29.53x (~10 hours)", ConsoleColor.Gray, false, false);
            ConsoleWindow.Print("time real-mars   - 1.027x (~20.5 min)", ConsoleColor.Gray, false, false);
            ConsoleWindow.Print("time real-europa - 3.551x (~1 hr 11 min)", ConsoleColor.Gray, false, false);
            ConsoleWindow.Print("time real-venus  - 116.75x (~39 hours)", ConsoleColor.Gray, false, false);
            ConsoleWindow.Print("time real-mimas  - 0.942x (~18.8 min)", ConsoleColor.Gray, false, false);
        }

        public static void RegisterCommands()
        {
            BeefsLongerOrbitalPeriodsPlugin.Log.LogInfo("  time [multiplier|preset] - Show or set day length");
            BeefsLongerOrbitalPeriodsPlugin.Log.LogInfo("  plants - Show plant settings");
            BeefsLongerOrbitalPeriodsPlugin.Log.LogInfo("  plants growth off|on|<value> - Control growth speed scaling");
            BeefsLongerOrbitalPeriodsPlugin.Log.LogInfo("  plants light on|off - Toggle light/dark requirement scaling");
        }
    }

    public static class OrbitalCommands
    {
        private static string GetTimeDescription(float multiplier)
        {
            double baseDayMinutes = 20.0;
            double newDayMinutes = baseDayMinutes * multiplier;

            if (newDayMinutes >= 60)
            {
                return $"~{newDayMinutes / 60:F1}hr";
            }
            if (newDayMinutes >= 1)
            {
                return $"~{newDayMinutes:F1}min";
            }
            return $"~{newDayMinutes * 60:F0}sec";
        }

        public static void SetPreset(DayLengthPreset preset)
        {
            if (NetworkManager.NetworkRole == NetworkRole.Client)
            {
                ConsoleWindow.Print("Cannot change orbital settings as client connected to server.", ConsoleColor.Red, false, false);
                return;
            }

            BeefsLongerOrbitalPeriodsPlugin.DayLengthPresetConfig.Value = preset;
            OrbitalPatcher.ApplyTimeCommand();

            float multiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveDayLengthMultiplier();
            string timeDesc = GetTimeDescription(multiplier);

            ConsoleWindow.Print($"Applied preset: {preset}", ConsoleColor.Cyan, false, false);
            ConsoleWindow.Print($"Multiplier: {multiplier}x, Day Length: {timeDesc}", ConsoleColor.Green, false, false);
        }

        public static void SetCustomMultiplier(float multiplier)
        {
            if (multiplier <= 0)
            {
                ConsoleWindow.Print("Invalid multiplier. Must be greater than 0.", ConsoleColor.Red, false, false);
                return;
            }

            if (NetworkManager.NetworkRole == NetworkRole.Client)
            {
                ConsoleWindow.Print("Cannot change orbital settings as client connected to server.", ConsoleColor.Red, false, false);
                return;
            }

            multiplier = Mathf.Clamp(multiplier, 0.01f, 100.0f);

            BeefsLongerOrbitalPeriodsPlugin.DayLengthPresetConfig.Value = DayLengthPreset.Custom;
            BeefsLongerOrbitalPeriodsPlugin.CustomDayLengthMultiplier.Value = multiplier;
            OrbitalPatcher.ApplyTimeCommand();

            string timeDesc = GetTimeDescription(multiplier);
            ConsoleWindow.Print($"Set to Custom mode", ConsoleColor.Cyan, false, false);
            ConsoleWindow.Print($"Multiplier: {multiplier}x, Day Length: {timeDesc}", ConsoleColor.Green, false, false);
        }

        public static void SetPlantGrowthMode(PlantGrowthMode mode)
        {
            if (NetworkManager.NetworkRole == NetworkRole.Client)
            {
                ConsoleWindow.Print("Cannot change plant settings as client connected to server.", ConsoleColor.Red, false, false);
                return;
            }

            BeefsLongerOrbitalPeriodsPlugin.PlantGrowthModeConfig.Value = mode;

            if (mode == PlantGrowthMode.Disabled)
            {
                ConsoleWindow.Print("Plant growth scaling DISABLED - vanilla growth speed", ConsoleColor.Green, false, false);
            }
            else if (mode == PlantGrowthMode.UseDayLength)
            {
                float dayMultiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveDayLengthMultiplier();
                ConsoleWindow.Print($"Plant growth scaling ENABLED (using day length)", ConsoleColor.Green, false, false);
                ConsoleWindow.Print($"Plants will grow {dayMultiplier}x slower", ConsoleColor.Yellow, false, false);
            }
            else if (mode == PlantGrowthMode.Custom)
            {
                float customMultiplier = BeefsLongerOrbitalPeriodsPlugin.PlantGrowthCustomMultiplier.Value;
                ConsoleWindow.Print($"Plant growth scaling ENABLED (custom)", ConsoleColor.Green, false, false);
                ConsoleWindow.Print($"Plants will grow {customMultiplier}x slower", ConsoleColor.Yellow, false, false);
            }
        }

        public static void SetPlantGrowthCustom(float multiplier)
        {
            if (NetworkManager.NetworkRole == NetworkRole.Client)
            {
                ConsoleWindow.Print("Cannot change plant settings as client connected to server.", ConsoleColor.Red, false, false);
                return;
            }

            if (multiplier <= 0)
            {
                ConsoleWindow.Print("Invalid multiplier. Must be greater than 0.", ConsoleColor.Red, false, false);
                return;
            }

            multiplier = Mathf.Clamp(multiplier, 0.01f, 100.0f);

            BeefsLongerOrbitalPeriodsPlugin.PlantGrowthModeConfig.Value = PlantGrowthMode.Custom;
            BeefsLongerOrbitalPeriodsPlugin.PlantGrowthCustomMultiplier.Value = multiplier;

            ConsoleWindow.Print($"Plant growth scaling set to CUSTOM", ConsoleColor.Green, false, false);
            ConsoleWindow.Print($"Plants will grow {multiplier}x slower", ConsoleColor.Yellow, false, false);
        }

        public static void SetPlantLightDarkScaling(bool enabled)
        {
            if (NetworkManager.NetworkRole == NetworkRole.Client)
            {
                ConsoleWindow.Print("Cannot change plant settings as client connected to server.", ConsoleColor.Red, false, false);
                return;
            }

            BeefsLongerOrbitalPeriodsPlugin.ScalePlantLightDark.Value = enabled;
            float multiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveDayLengthMultiplier();

            if (enabled)
            {
                ConsoleWindow.Print($"Plant LIGHT/DARK scaling ENABLED", ConsoleColor.Green, false, false);
                ConsoleWindow.Print($"Plants need {multiplier}x more light and {multiplier}x more darkness per day", ConsoleColor.Yellow, false, false);
            }
            else
            {
                ConsoleWindow.Print("Plant LIGHT/DARK scaling DISABLED - vanilla requirements", ConsoleColor.Green, false, false);
            }
        }

        public static void ShowPlantSettings()
        {
            PlantGrowthMode growthMode = BeefsLongerOrbitalPeriodsPlugin.PlantGrowthModeConfig.Value;
            bool lightEnabled = BeefsLongerOrbitalPeriodsPlugin.ScalePlantLightDark.Value;
            float dayMultiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveDayLengthMultiplier();
            float growthMultiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectivePlantGrowthMultiplier();

            ConsoleWindow.Print("=== Plant Settings ===", ConsoleColor.Yellow, false, false);
            ConsoleWindow.Print($"Current day length multiplier: {dayMultiplier}x", ConsoleColor.White, false, false);
            ConsoleWindow.Print("", ConsoleColor.White, false, false);

            // Growth speed status
            if (growthMode == PlantGrowthMode.Disabled)
            {
                ConsoleWindow.Print($"Growth Speed Scaling: DISABLED", ConsoleColor.Red, false, false);
                ConsoleWindow.Print($"  → Plants use vanilla growth speed", ConsoleColor.Gray, false, false);
            }
            else if (growthMode == PlantGrowthMode.UseDayLength)
            {
                ConsoleWindow.Print($"Growth Speed Scaling: ENABLED (using day length)", ConsoleColor.Green, false, false);
                ConsoleWindow.Print($"  → Plants grow {growthMultiplier}x slower than vanilla", ConsoleColor.Gray, false, false);
            }
            else if (growthMode == PlantGrowthMode.Custom)
            {
                ConsoleWindow.Print($"Growth Speed Scaling: ENABLED (custom)", ConsoleColor.Green, false, false);
                ConsoleWindow.Print($"  → Plants grow {growthMultiplier}x slower than vanilla", ConsoleColor.Gray, false, false);
            }

            ConsoleWindow.Print("", ConsoleColor.White, false, false);

            // Light/dark status
            if (lightEnabled)
            {
                ConsoleWindow.Print($"Light/Dark Scaling: ENABLED", ConsoleColor.Green, false, false);
                ConsoleWindow.Print($"  → Plants need {dayMultiplier}x more light/dark per day cycle", ConsoleColor.Gray, false, false);
            }
            else
            {
                ConsoleWindow.Print($"Light/Dark Scaling: DISABLED", ConsoleColor.Red, false, false);
                ConsoleWindow.Print($"  → Plants use vanilla light/dark requirements", ConsoleColor.Gray, false, false);
            }
        }

        public static void ShowCurrentSettings()
        {
            DayLengthPreset preset = BeefsLongerOrbitalPeriodsPlugin.DayLengthPresetConfig.Value;
            float current = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveDayLengthMultiplier();
            PlantGrowthMode growthMode = BeefsLongerOrbitalPeriodsPlugin.PlantGrowthModeConfig.Value;
            bool lightScaling = BeefsLongerOrbitalPeriodsPlugin.ScalePlantLightDark.Value;

            ConsoleWindow.Print("=== Beef's Longer Orbital Periods Settings ===", ConsoleColor.Yellow, false, false);
            ConsoleWindow.Print($"Day Length Preset: {preset}", ConsoleColor.White, false, false);
            ConsoleWindow.Print($"Effective Multiplier: {current}x", ConsoleColor.White, false, false);

            string timeDesc = GetTimeDescription(current);
            ConsoleWindow.Print($"Day Length: {timeDesc}", ConsoleColor.White, false, false);

            string growthDesc;
            if (growthMode == PlantGrowthMode.Disabled)
            {
                growthDesc = "Disabled";
            }
            else if (growthMode == PlantGrowthMode.UseDayLength)
            {
                growthDesc = $"Using Day Length ({current}x)";
            }
            else
            {
                growthDesc = $"Custom ({BeefsLongerOrbitalPeriodsPlugin.PlantGrowthCustomMultiplier.Value}x)";
            }

            ConsoleWindow.Print($"Plant Growth Scaling: {growthDesc}", ConsoleColor.White, false, false);
            ConsoleWindow.Print($"Plant Light/Dark Scaling: {(lightScaling ? "Enabled" : "Disabled")}", ConsoleColor.White, false, false);
        }
    }
}