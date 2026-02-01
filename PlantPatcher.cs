using HarmonyLib;
using System;
using System.Reflection;
using Assets.Scripts.Networking;
using Assets.Scripts.Genetics;
using Assets.Scripts.Objects.Items;
using Genetics;

namespace BeefsLongerOrbitalPeriods
{
    [HarmonyPatch(typeof(PlantLifeRequirements), nameof(PlantLifeRequirements.GrowthEfficiency))]
    public static class GrowthEfficiencyPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            if (NetworkManager.NetworkRole == NetworkRole.Client)
            {
                return;
            }

            if (!BeefsLongerOrbitalPeriodsPlugin.IsPlantGrowthScalingEnabled())
            {
                return;
            }

            float multiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectivePlantGrowthMultiplier();
            if (multiplier == 1.0f)
            {
                return;
            }

            __result /= multiplier;
        }
    }

    [HarmonyPatch]
    public static class PlantStatPatcher
    {
        private static FieldInfo _geneField;
        private static bool _initialized;

        static PlantStatPatcher()
        {
            try
            {
                _geneField = typeof(PlantStat).GetField("_gene", BindingFlags.NonPublic | BindingFlags.Instance);
                _initialized = _geneField != null;
                if (!_initialized)
                {
                    UnityEngine.Debug.LogWarning("[BeefsLongerOrbitalPeriods] Could not find PlantStat._gene field");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[BeefsLongerOrbitalPeriods] PlantStatPatcher init: {ex.Message}");
            }
        }

        [HarmonyPatch(typeof(PlantStat), "Get", new Type[] { })]
        [HarmonyPostfix]
        public static void Postfix(PlantStat __instance, ref float __result)
        {
            if (!_initialized)
            {
                return;
            }

            if (NetworkManager.NetworkRole == NetworkRole.Client)
            {
                return;
            }

            if (!BeefsLongerOrbitalPeriodsPlugin.ScalePlantLightDark.Value)
            {
                return;
            }

            float multiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveDayLengthMultiplier();
            if (multiplier == 1.0f)
            {
                return;
            }

            try
            {
                Gene gene = (Gene)_geneField.GetValue(__instance);
                if (gene == Gene.LightPerDay || gene == Gene.DarkPerDay)
                {
                    __result *= multiplier;
                }
            }
            catch
            { }
        }
    }
}