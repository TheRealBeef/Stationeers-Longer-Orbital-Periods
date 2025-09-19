using HarmonyLib;
using UnityEngine;
using System;
using Assets.Scripts.Networking;
using Assets.Scripts;
using Util.Commands;

namespace BeefsLongerOrbitalPeriods
{
    [HarmonyPatch]
    public class ConsoleCommandHandler
    {
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

            switch (command)
            {
                case "time":
                {
                    if (parts.Length == 1)
                    {
                        OrbitalCommands.ShowCurrentSettings();
                    }

                    else if (parts.Length == 2 && float.TryParse(parts[1], out float multiplier))
                    {
                        OrbitalCommands.SetTimeMultiplier(multiplier);
                        return false;
                    }

                    ShowUsage();
                    return false;
                }

                default:
                    return true;
            }
        }

        private static void ShowUsage()
        {
            ConsoleWindow.Print("Usage: time <multiplier>", ConsoleColor.Red, false, false);
            ConsoleWindow.Print("Example: time 6.0     (sets a 2-hour day) - using time by itself shows this dialog as well as current settings", ConsoleColor.Green, false, false);
            ConsoleWindow.Print("0.5x = 10min | 1x = 20min | 3x = 1 hour (mod default) | 10x = 3hr 20min", ConsoleColor.Gray, false, false);
        }

        public static void RegisterCommands()
        {
            BeefsLongerOrbitalPeriodsPlugin.Log.LogInfo("Console command registered:");
            BeefsLongerOrbitalPeriodsPlugin.Log.LogInfo("  time [multiplier] - Show or set orbital period multiplier");
        }
    }

    public static class OrbitalCommands
    {
        private static string GetTimeDescription(float multiplier)
        {
            double baseDayMinutes = 20.0;
            double newDayMinutes = baseDayMinutes * multiplier;

            if (newDayMinutes >= 60)
                return $"~{newDayMinutes / 60:F1}hr";
            if (newDayMinutes >= 1)
                return $"~{newDayMinutes:F1}min";

            return $"~{newDayMinutes * 60:F0}sec";
        }

        public static void SetTimeMultiplier(float multiplier)
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

            BeefsLongerOrbitalPeriodsPlugin.OrbitalPeriodMultiplier.Value = multiplier;
            OrbitalPatcher.ApplyTimeCommand();
            string timeDesc = GetTimeDescription(multiplier);
            ConsoleWindow.Print($"Multiplier: {multiplier}x, Day Length: {timeDesc}", ConsoleColor.Red, false, false);
        }

        public static void ShowCurrentSettings()
        {
            float current = BeefsLongerOrbitalPeriodsPlugin.OrbitalPeriodMultiplier.Value;
            ConsoleWindow.Print("=== Beef's Longer Orbital Periods Settings ===", ConsoleColor.Yellow, false, false);
            // NetworkRole networkRole = NetworkManager.NetworkRole;
            // ConsoleWindow.Print($"Network Role: {networkRole}", ConsoleColor.White, false, false);
            ConsoleWindow.Print($"Orbital Multiplier: {current}x", ConsoleColor.Red, false, false);
            string timeDesc = GetTimeDescription(current);
            ConsoleWindow.Print($"Day Length: {timeDesc}", ConsoleColor.Red, false, false);

        }
    }
}