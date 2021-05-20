using System;
using HarmonyLib;
using UnityEngine;
using Assets.Scripts;

namespace OrbitalPeriod
{
	
	// Fixes to SSAO
	[HarmonyPatch(typeof(CursorManager), "AssignSun")]
    public class OrbitalPatcher
    {
        static void Prefix()
        {
            // Current multiplier is 10
            // Orbital Period of 0.1 will be 20 minutes
            // Orbital period of 1.0 will be 3 hours 20 minutes
            // Orbital Period of 2.0 will be 6 hours 40 minutes
            
            float BaseOrbitalPeriodMultiplier = 10.0f;

            LongerOrbitalPeriods.Orb_Period.OrbitalPeriod.AppendLog("Applying Orbital Period Settings");

            CursorManager.Instance.BaseSunOrbitPeriod *= BaseOrbitalPeriodMultiplier;

            LongerOrbitalPeriods.Orb_Period.OrbitalPeriod.AppendLog("Orbital Period Set - New settings are " + BaseOrbitalPeriodMultiplier.ToString() + "x longer");
        }
    }
}
