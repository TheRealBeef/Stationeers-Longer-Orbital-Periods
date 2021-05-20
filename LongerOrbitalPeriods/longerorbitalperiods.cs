using System;
using HarmonyLib;
using BepInEx;
using UnityEngine;

namespace LongerOrbitalPeriods
{
    public class OrbPeriod
    {

        [BepInPlugin("org.bepinex.plugins.orbitalperiod", "Beef's Longer Orbital Periods", "0.1")]
        [BepInProcess("rocketstation.exe")]
        public class init : BaseUnityPlugin
        {
            void Awake()
            {
                OrbitalPeriod.Awake();
            }
        }

        public class OrbitalPeriod
        {
            // Variable Definitions

            public static void AppendLog(string logdetails)
            {
                UnityEngine.Debug.Log("Beef's Longer Orbital Periods - " + logdetails);
            }

            // Awake is called once when both the game and the plug-in are loaded
            public static void Awake()
            {
                //Initialize();

                AppendLog("Initialized");
                var harmony = new Harmony("org.bepinex.plugins.orbitalperiod");
                harmony.PatchAll();
                AppendLog("Patched with Harmony");
            }
        }
    }

}
