using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace BeefsLongerOrbitalPeriods
{
    public enum DayLengthPreset
    {
        Custom,
        RealMoon,
        RealMars,
        RealEuropa,
        RealVenus,
        RealMimas
    }

    public enum PlantGrowthMode
    {
        Disabled,
        UseDayLength,
        Custom
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class BeefsLongerOrbitalPeriodsPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<DayLengthPreset> DayLengthPresetConfig;
        public static ConfigEntry<float> CustomDayLengthMultiplier;
        public static ConfigEntry<PlantGrowthMode> PlantGrowthModeConfig;
        public static ConfigEntry<float> PlantGrowthCustomMultiplier;
        public static ConfigEntry<bool> ScalePlantLightDark;
        public static ManualLogSource Log;

        public static readonly float PresetMoon = 29.53f;
        public static readonly float PresetMars = 1.027f;
        public static readonly float PresetEuropa = 3.551f;
        public static readonly float PresetVenus = 116.75f;
        public static readonly float PresetMimas = 0.942f;

        public static float GetEffectiveDayLengthMultiplier()
        {
            if (DayLengthPresetConfig.Value == DayLengthPreset.RealMoon)
            {
                return PresetMoon;
            }
            if (DayLengthPresetConfig.Value == DayLengthPreset.RealMars)
            {
                return PresetMars;
            }
            if (DayLengthPresetConfig.Value == DayLengthPreset.RealEuropa)
            {
                return PresetEuropa;
            }
            if (DayLengthPresetConfig.Value == DayLengthPreset.RealVenus)
            {
                return PresetVenus;
            }
            if (DayLengthPresetConfig.Value == DayLengthPreset.RealMimas)
            {
                return PresetMimas;
            }
            return CustomDayLengthMultiplier.Value;
        }

        public static float GetEffectivePlantGrowthMultiplier()
        {
            if (PlantGrowthModeConfig.Value == PlantGrowthMode.Disabled)
            {
                return 1.0f;
            }
            if (PlantGrowthModeConfig.Value == PlantGrowthMode.UseDayLength)
            {
                return GetEffectiveDayLengthMultiplier();
            }
            if (PlantGrowthModeConfig.Value == PlantGrowthMode.Custom)
            {
                return PlantGrowthCustomMultiplier.Value;
            }
            return 1.0f;
        }

        public static bool IsPlantGrowthScalingEnabled()
        {
            return PlantGrowthModeConfig.Value != PlantGrowthMode.Disabled;
        }

        private void Awake()
        {
            Log = Logger;

            DayLengthPresetConfig = Config.Bind(
                "1. Day Length",
                "Preset",
                DayLengthPreset.Custom,
                "Select a real-world day length preset, or Custom to use your own value.\n" +
                "• Custom: Use the multiplier below\n" +
                "• RealMoon: 29.53x (~10 hours)\n" +
                "• RealMars: 1.027x (~20.5 min)\n" +
                "• RealEuropa: 3.551x (~1 hr 11 min)\n" +
                "• RealVenus: 116.75x (~39 hours)\n" +
                "• RealMimas: 0.942x (~18.8 min)");

            CustomDayLengthMultiplier = Config.Bind(
                "1. Day Length",
                "Custom Multiplier",
                3.0f,
                new ConfigDescription(
                    "Custom day length multiplier (only used when Preset is 'Custom').\n" +
                    "Base day/night cycle is 20 minutes.\n" +
                    "Examples: 0.5x = 10min, 1x = 20min, 3x = 1hr, 6x = 2hr",
                    new AcceptableValueRange<float>(0.01f, 100.0f)));

            PlantGrowthModeConfig = Config.Bind(
                "2. Plant Growth",
                "Growth Speed Scaling",
                PlantGrowthMode.Disabled,
                "Controls how plant growth speed is scaled.\n" +
                "• Disabled: Vanilla growth speed\n" +
                "• UseDayLength: Plants grow slower by the day length multiplier\n" +
                "• Custom: Use a custom growth multiplier below\n\n" +
                "WARNING: High multipliers make plants take a very long time to grow");

            PlantGrowthCustomMultiplier = Config.Bind(
                "2. Plant Growth",
                "Custom Growth Multiplier",
                3.0f,
                new ConfigDescription(
                    "Custom plant growth multiplier (only used when Growth Speed Scaling is 'Custom').\n" +
                    "Plants will grow this many times slower than vanilla.",
                    new AcceptableValueRange<float>(0.01f, 100.0f)));

            ScalePlantLightDark = Config.Bind(
                "3. Plant Light and Dark",
                "Scale Light/Dark Requirements",
                false,
                "If enabled, plant light/darkness requirements per day will be scaled by the day length multiplier.\n" +
                "This makes plants need proportionally more light/dark time per longer day cycle.");

            float dayMultiplier = GetEffectiveDayLengthMultiplier();
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} loaded");
            Log.LogInfo($"Day length: {DayLengthPresetConfig.Value} ({dayMultiplier}x)");
            Log.LogInfo($"Plant growth: {PlantGrowthModeConfig.Value}");
            Log.LogInfo($"Plant light/dark scaling: {(ScalePlantLightDark.Value ? "Enabled" : "Disabled")}");

            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            ConsoleCommandHandler.RegisterCommands();

            Log.LogInfo("Console commands: 'time', 'plants'");
        }

        public static void AppendLog(string logdetails)
        {
            Log.LogInfo(logdetails);
        }
    }
}