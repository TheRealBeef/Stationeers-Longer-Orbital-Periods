using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace BeefsLongerOrbitalPeriods
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class BeefsLongerOrbitalPeriodsPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> OrbitalPeriodMultiplier;
        public static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;

            OrbitalPeriodMultiplier = Config.Bind("General",
                "OrbitalPeriodMultiplier",
                3.0f,
                "Multiplier for orbital periods. Base day/night cycle is 20 minutes. Examples: 0.5x = 10min, 1x = 20min, 3x = 1hr, 6x = 2hr, 12x = 4hr. Note: Does not affect storm duration.");

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded with multiplier: {OrbitalPeriodMultiplier.Value}x");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            ConsoleCommandHandler.RegisterCommands();

            Log.LogInfo("Use F3 console command: 'time <value>'");
        }

        public static void AppendLog(string logdetails)
        {
            Log.LogInfo(logdetails);
        }
    }
}