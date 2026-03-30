using HarmonyLib;
using System;
using Assets.Scripts;
using Assets.Scripts.Networking;
using Weather;

namespace BeefsLongerOrbitalPeriods
{
    [HarmonyPatch(typeof(WeatherManager), nameof(WeatherManager.ScheduleWeatherEvent))]
    public static class WeatherSchedulePatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (NetworkManager.NetworkRole == NetworkRole.Client)
            {
                return;
            }

            float durationMultiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveStormDurationMultiplier();
            if (durationMultiplier != 1.0f)
            {
                WeatherManager.WeatherEventLength *= durationMultiplier;
            }

            float dayLengthMultiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveDayLengthMultiplier();
            if (dayLengthMultiplier != 1.0f)
            {
                float offset = WeatherManager.WeatherStartTime - GameManager.GameTime;
                float scaledOffset = offset * dayLengthMultiplier;

                float dayLengthSeconds = OrbitalSimulation.GetDayLengthSeconds();
                float maxOffset = dayLengthSeconds * 0.9f;
                if (scaledOffset > maxOffset)
                {
                    scaledOffset = maxOffset;
                }

                WeatherManager.WeatherStartTime = GameManager.GameTime + scaledOffset;
            }
        }
    }

    [HarmonyPatch(typeof(WeatherManager), nameof(WeatherManager.ImmediatelyActivateWeatherEvent), new Type[] { typeof(WeatherEvent) })]
    public static class WeatherImmediatePatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (NetworkManager.NetworkRole == NetworkRole.Client)
            {
                return;
            }

            float durationMultiplier = BeefsLongerOrbitalPeriodsPlugin.GetEffectiveStormDurationMultiplier();
            if (durationMultiplier != 1.0f)
            {
                WeatherManager.WeatherEventLength *= durationMultiplier;
            }
        }
    }
}