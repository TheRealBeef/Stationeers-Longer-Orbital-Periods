using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Networking;
using Assets.Scripts.Genetics;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.UI;
using Assets.Scripts.UI.Genetics;
using Genetics;
using UnityEngine;

namespace BeefsLongerOrbitalPeriods
{
    [HarmonyPatch(typeof(OrbitalSimulation), nameof(OrbitalSimulation.GetDayLengthSeconds))]
    public static class DayLengthSecondsPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref int __result)
        {
            float multiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveDayLengthMultiplier();
            if (multiplier == 1.0f)
            {
                return;
            }

            __result = Mathf.RoundToInt(__result * multiplier);
        }
    }
    public static class PlantGrowthTimeScaler
    {
        private static readonly Dictionary<PlantStage, float> _prefabOriginalLengths = new Dictionary<PlantStage, float>();
        private static bool _isApplied = false;

        public static void ApplyScaling()
        {
            if (!BeefsLongerOrbitalPeriodsPlugin.IsPlantGrowthScalingEnabled())
            {
                if (_isApplied)
                {
                    RestoreAll();
                }
                return;
            }

            float multiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectivePlantGrowthMultiplier();
            if (multiplier == 1.0f)
            {
                if (_isApplied)
                {
                    RestoreAll();
                }
                return;
            }

            if (Plant.AllPlantPrefabs == null || Plant.AllPlantPrefabs.Count == 0)
            {
                return;
            }

            foreach (Plant plant in Plant.AllPlantPrefabs)
            {
                if (plant == null || plant.GrowthStates == null) continue;

                foreach (PlantStage stage in plant.GrowthStates)
                {
                    if (stage == null) continue;

                    if (!_prefabOriginalLengths.ContainsKey(stage))
                    {
                        _prefabOriginalLengths[stage] = stage.Length;
                    }

                    stage.Length = _prefabOriginalLengths[stage] * multiplier;
                }
            }
            
            if (Plant.AllPlants != null)
            {
                foreach (Plant plant in Plant.AllPlants)
                {
                    if (plant == null || plant.GrowthStates == null) continue;
                    ScaleLivePlantStages(plant, multiplier);
                }
            }

            _isApplied = true;
        }

        private static void ScaleLivePlantStages(Plant livePlant, float multiplier)
        {
            Plant sourcePrefab = livePlant.SourcePrefab as Plant;
            if (sourcePrefab == null || sourcePrefab.GrowthStates == null)
            {
                return;
            }

            int maxIndex = Math.Min(livePlant.GrowthStates.Count, sourcePrefab.GrowthStates.Count);
            for (int i = 0; i < maxIndex; i++)
            {
                PlantStage liveStage = livePlant.GrowthStates[i];
                PlantStage prefabStage = sourcePrefab.GrowthStates[i];

                if (liveStage == null || prefabStage == null) continue;

                float original;
                if (_prefabOriginalLengths.TryGetValue(prefabStage, out original))
                {
                    liveStage.Length = original * multiplier;
                }
            }
        }

        public static void RestoreAll()
        {
            if (!_isApplied) return;

            foreach (var kvp in _prefabOriginalLengths)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.Length = kvp.Value;
                }
            }

            if (Plant.AllPlants != null)
            {
                foreach (Plant plant in Plant.AllPlants)
                {
                    if (plant == null || plant.GrowthStates == null) continue;

                    Plant sourcePrefab = plant.SourcePrefab as Plant;
                    if (sourcePrefab == null || sourcePrefab.GrowthStates == null) continue;

                    int maxIndex = Math.Min(plant.GrowthStates.Count, sourcePrefab.GrowthStates.Count);
                    for (int i = 0; i < maxIndex; i++)
                    {
                        PlantStage liveStage = plant.GrowthStates[i];
                        PlantStage prefabStage = sourcePrefab.GrowthStates[i];

                        if (liveStage == null || prefabStage == null) continue;

                        float original;
                        if (_prefabOriginalLengths.TryGetValue(prefabStage, out original))
                        {
                            liveStage.Length = original;
                        }
                    }
                }
            }

            _isApplied = false;
        }

        public static void ClearCache()
        {
            RestoreAll();
            _prefabOriginalLengths.Clear();
        }

        public static void RefreshAfterConfigChange()
        {
            ApplyScaling();

            if (Stationpedia.Instance != null)
            {
                Stationpedia.Regenerate();
            }
        }

        public static float GetOriginalLength(PlantStage stage)
        {
            if (stage == null) return 0f;

            float original;
            if (_prefabOriginalLengths.TryGetValue(stage, out original))
            {
                return original;
            }

            return stage.Length;
        }
    }

    [HarmonyPatch(typeof(OrbitalSimulation), "Load")]
    public static class PostWorldLoadPatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            PlantGrowthTimeScaler.RefreshAfterConfigChange();
        }
    }

    [HarmonyPatch(typeof(OrbitalSimulation), "Clear")]
    public static class PlantGrowthClearPatcher
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            PlantGrowthTimeScaler.ClearCache();
        }
    }

    [HarmonyPatch(typeof(OrbitalPatcher), nameof(OrbitalPatcher.ApplyTimeCommand))]
    public static class PostTimeCommandPatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            PlantGrowthTimeScaler.RefreshAfterConfigChange();
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
                if (gene == Gene.LightPerDay || gene == Gene.DarkPerDay ||
                    gene == Gene.LightTolerance || gene == Gene.DarknessTolerance)
                {
                    __result *= multiplier;
                }
            }
            catch
            { }
        }
    }

    [HarmonyPatch(typeof(PlantLifeRequirements), nameof(PlantLifeRequirements.AddToStationpedia))]
    public static class StationpediaPatcher
    {
        private static PropertyInfo _baseProp;
        private static PropertyInfo _minProp;
        private static PropertyInfo _maxProp;
        private static bool _initialized;

        static StationpediaPatcher()
        {
            try
            {
                _baseProp = typeof(PlantStat).GetProperty("Base", BindingFlags.Public | BindingFlags.Instance);
                _minProp = typeof(PlantStat).GetProperty("Min", BindingFlags.Public | BindingFlags.Instance);
                _maxProp = typeof(PlantStat).GetProperty("Max", BindingFlags.Public | BindingFlags.Instance);

                _initialized = _baseProp != null && _baseProp.GetSetMethod(true) != null
                            && _minProp != null && _minProp.GetSetMethod(true) != null
                            && _maxProp != null && _maxProp.GetSetMethod(true) != null;

                if (!_initialized)
                {
                    UnityEngine.Debug.LogWarning("[BeefsLongerOrbitalPeriods] Could not find PlantStat Base/Min/Max property setters");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[BeefsLongerOrbitalPeriods] StationpediaPatcher init: {ex.Message}");
                _initialized = false;
            }
        }

        [HarmonyPrefix]
        public static void Prefix(PlantLifeRequirements __instance, out float[] __state)
        {
            __state = null;

            if (!_initialized || !BeefsLongerOrbitalPeriodsPlugin.ScalePlantLightDark.Value)
            {
                return;
            }

            float multiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveDayLengthMultiplier();
            if (multiplier == 1.0f)
            {
                return;
            }

            __state = new float[]
            {
                __instance.LightPerDay.Base,
                __instance.LightPerDay.Min,
                __instance.LightPerDay.Max,
                __instance.DarknessPerDay.Base,
                __instance.DarknessPerDay.Min,
                __instance.DarknessPerDay.Max,
                __instance.TimeUntilLightDamage.Base,
                __instance.TimeUntilLightDamage.Min,
                __instance.TimeUntilLightDamage.Max,
                __instance.TimeUntilDarknessDamage.Base,
                __instance.TimeUntilDarknessDamage.Min,
                __instance.TimeUntilDarknessDamage.Max
            };

            try
            {
                var baseSetter = _baseProp.GetSetMethod(true);
                var minSetter = _minProp.GetSetMethod(true);
                var maxSetter = _maxProp.GetSetMethod(true);

                baseSetter.Invoke(__instance.LightPerDay, new object[] { __state[0] * multiplier });
                minSetter.Invoke(__instance.LightPerDay, new object[] { __state[1] * multiplier });
                maxSetter.Invoke(__instance.LightPerDay, new object[] { __state[2] * multiplier });

                baseSetter.Invoke(__instance.DarknessPerDay, new object[] { __state[3] * multiplier });
                minSetter.Invoke(__instance.DarknessPerDay, new object[] { __state[4] * multiplier });
                maxSetter.Invoke(__instance.DarknessPerDay, new object[] { __state[5] * multiplier });

                baseSetter.Invoke(__instance.TimeUntilLightDamage, new object[] { __state[6] * multiplier });
                minSetter.Invoke(__instance.TimeUntilLightDamage, new object[] { __state[7] * multiplier });
                maxSetter.Invoke(__instance.TimeUntilLightDamage, new object[] { __state[8] * multiplier });

                baseSetter.Invoke(__instance.TimeUntilDarknessDamage, new object[] { __state[9] * multiplier });
                minSetter.Invoke(__instance.TimeUntilDarknessDamage, new object[] { __state[10] * multiplier });
                maxSetter.Invoke(__instance.TimeUntilDarknessDamage, new object[] { __state[11] * multiplier });
            }
            catch
            {
                __state = null;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(PlantLifeRequirements __instance, float[] __state)
        {
            if (__state == null || !_initialized)
            {
                return;
            }

            try
            {
                var baseSetter = _baseProp.GetSetMethod(true);
                var minSetter = _minProp.GetSetMethod(true);
                var maxSetter = _maxProp.GetSetMethod(true);

                baseSetter.Invoke(__instance.LightPerDay, new object[] { __state[0] });
                minSetter.Invoke(__instance.LightPerDay, new object[] { __state[1] });
                maxSetter.Invoke(__instance.LightPerDay, new object[] { __state[2] });

                baseSetter.Invoke(__instance.DarknessPerDay, new object[] { __state[3] });
                minSetter.Invoke(__instance.DarknessPerDay, new object[] { __state[4] });
                maxSetter.Invoke(__instance.DarknessPerDay, new object[] { __state[5] });

                baseSetter.Invoke(__instance.TimeUntilLightDamage, new object[] { __state[6] });
                minSetter.Invoke(__instance.TimeUntilLightDamage, new object[] { __state[7] });
                maxSetter.Invoke(__instance.TimeUntilLightDamage, new object[] { __state[8] });

                baseSetter.Invoke(__instance.TimeUntilDarknessDamage, new object[] { __state[9] });
                minSetter.Invoke(__instance.TimeUntilDarknessDamage, new object[] { __state[10] });
                maxSetter.Invoke(__instance.TimeUntilDarknessDamage, new object[] { __state[11] });
            }
            catch
            { }
        }
    }

    [HarmonyPatch]
    public static class StationpediaGrowthTimePatcher
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return typeof(Stationpedia).GetMethod("AddGrowthTime",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [HarmonyPostfix]
        public static void Postfix(Thing prefab, ref StationpediaPage page)
        {
            if (!BeefsLongerOrbitalPeriodsPlugin.IsPlantGrowthScalingEnabled())
            {
                return;
            }

            Plant plant = prefab as Plant;
            if (plant == null || plant is Seed)
            {
                return;
            }

            float multiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectivePlantGrowthMultiplier();
            if (multiplier == 1.0f)
            {
                return;
            }

            float vanillaTotal = 0f;
            for (int i = 0; i < plant.GrowthStates.Count; i++)
            {
                PlantStage stage = plant.GrowthStates[i];
                if (stage == null || stage.Mature)
                {
                    break;
                }
                float original = PlantGrowthTimeScaler.GetOriginalLength(stage);
                if (original <= 0f)
                {
                    break;
                }
                vanillaTotal += original;
            }

            if (vanillaTotal > 0f)
            {
                page.GrowthTime = ValueDisplay.GetUnitValue(vanillaTotal * multiplier, ValueDisplay.Unit.Time);
            }
        }
    }

    [HarmonyPatch(typeof(PlantSample), MethodType.Constructor, new Type[] { typeof(Plant) })]
    public static class PlantSamplePatcher
    {
        [HarmonyPostfix]
        public static void Postfix(PlantSample __instance)
        {
            if (!BeefsLongerOrbitalPeriodsPlugin.ScalePlantLightDark.Value)
            {
                return;
            }

            float multiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveDayLengthMultiplier();
            if (multiplier == 1.0f)
            {
                return;
            }

            __instance.LightPerDay = ScaleWrapperBaseMinMax(__instance.LightPerDay, multiplier);
            __instance.DarknessPerDay = ScaleWrapperBaseMinMax(__instance.DarknessPerDay, multiplier);
        }

        private static RequirementWrapper ScaleWrapperBaseMinMax(RequirementWrapper wrapper, float multiplier)
        {
            return new RequirementWrapper(
                wrapper.BaseValue * multiplier,
                wrapper.MinValue * multiplier,
                wrapper.MaxValue * multiplier,
                wrapper.CurrentValue
            );
        }
    }
}