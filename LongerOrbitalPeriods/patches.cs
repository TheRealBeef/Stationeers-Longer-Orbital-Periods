using System;
using HarmonyLib;
using UnityEngine;
using Assets.Scripts;

namespace Orbital_Period
{
	
	// Fixes to SSAO
	[HarmonyPatch(typeof(CursorManager), "Initialize")]
    public class OrbitalPatcher
    {
        static void Prefix()
        {
            // Current multiplier is 20
            // Orbital Period of 0.1 will be 40 minutes
            // Orbital period of 1.0 will be 6 hours 40 minutes
            // Orbital Period of 2.0 will be 13 hours 40 minutes

            float BaseOrbitalPeriodMultiplier = 20.0f;

            BeefPeriod.OrbitalPeriod.AppendLog("Applying Orbital Period Settings");

            CursorManager.Instance.BaseSunOrbitPeriod *= BaseOrbitalPeriodMultiplier;

            BeefPeriod.OrbitalPeriod.AppendLog("Orbital Period Set - New settings are " + BaseOrbitalPeriodMultiplier.ToString() + "x longer");
        }
    }
}
