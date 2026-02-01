using HarmonyLib;
using UnityEngine;
using System.Reflection;
using Assets.Scripts.Networking;
using Assets.Scripts;
using System;

namespace BeefsLongerOrbitalPeriods
{
    [HarmonyPatch(typeof(OrbitalSimulation), "Load")]
    public class OrbitalPatcher
    {
        private static bool hasBeenApplied = false;
        private static double? cachedOriginalTimeScale;

        public static void ApplyTimeCommand()
        {
            if (cachedOriginalTimeScale == null) return;

            // Don't apply on clients or weird things could happen ...
            if (NetworkManager.NetworkRole == NetworkRole.Client) return;

            float multiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveDayLengthMultiplier();
            Console.WriteLine($"[BeefsLongerOrbitalPeriods] Applying modification from command. Multiplier: {multiplier}x");

            FieldInfo timeScaleField = typeof(OrbitalSimulation).GetField("_timeScale", BindingFlags.NonPublic | BindingFlags.Instance);
            if (timeScaleField != null)
            {
                double newTimeScale = cachedOriginalTimeScale.Value / multiplier;
                timeScaleField.SetValue(OrbitalSimulation.System, newTimeScale);
                Console.WriteLine($"[BeefsLongerOrbitalPeriods] *** COMMAND APPLIED SUCCESSFULLY ***");
            }
        }

        [HarmonyPostfix]
        static void ModifyOrbitalSpeed(WorldSetting worldSetting)
        {
            // Cache OG timescale in case the game goes with something inconsistent in the future
            if (cachedOriginalTimeScale == null && OrbitalSimulation.System != null)
            {
                cachedOriginalTimeScale = OrbitalSimulation.System.TimeScale;
            }

            if (hasBeenApplied)
            {
                Console.WriteLine($"[BeefsLongerOrbitalPeriods] Duplicate call to OrbitalSimulation.Load - skipping");
                return;
            }

            // Dont run on client to avoid sheninagans
            NetworkRole networkRole = NetworkManager.NetworkRole;
            if (networkRole == NetworkRole.Client)
            {
                return;
            }

            float multiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveDayLengthMultiplier();
            Console.WriteLine($"[BeefsLongerOrbitalPeriods] Applying Timescale - Multiplier: {multiplier}x");

            FieldInfo timeScaleField = typeof(OrbitalSimulation).GetField("_timeScale", BindingFlags.NonPublic | BindingFlags.Instance);
            if (timeScaleField != null)
            {
                double newTimeScale = cachedOriginalTimeScale.Value / multiplier;
                timeScaleField.SetValue(OrbitalSimulation.System, newTimeScale);
                Console.WriteLine($"[BeefsLongerOrbitalPeriods] *** TIMESCALE PATCH APPLIED SUCCESSFULLY ***");
                hasBeenApplied = true;
            }
            else
            {
                Console.WriteLine("[BeefsLongerOrbitalPeriods] ERROR: Could not find timeScale field for modification");
                hasBeenApplied = true;
            }
        }
    }

    [HarmonyPatch(typeof(OrbitalSimulation), "Clear")]
    public class OrbitalPatcher_Clear
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            var hasBeenAppliedField = typeof(OrbitalPatcher).GetField("hasBeenApplied", BindingFlags.NonPublic | BindingFlags.Static);
            if (hasBeenAppliedField != null)
            {
                hasBeenAppliedField.SetValue(null, false);
            }

            var cachedTimeScaleField = typeof(OrbitalPatcher).GetField("cachedOriginalTimeScale", BindingFlags.NonPublic | BindingFlags.Static);
            if (cachedTimeScaleField != null)
            {
                cachedTimeScaleField.SetValue(null, null);
            }
        }
    }
}