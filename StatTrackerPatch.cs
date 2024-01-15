using HarmonyLib;
using UnityEngine;

namespace LethalCompanyStatTracker {
    internal class StatTrackerPatch {
        private static StatisticsTracker Tracker;

        [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
        [HarmonyPostfix]
        static void OnMoonStart() {
            try {
                if (Tracker == null) {
                    Tracker = GameObject.FindObjectOfType<StatisticsTracker>();
                }
                Tracker.SnapshotCollectedItemsOnMoonStart();
            } catch (System.Exception e) {
                LogError(e, nameof(OnMoonStart));
            }
        }

        [HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
        [HarmonyPrefix]
        static void OnMoonExit(RoundManager __instance) {
            try {
                Tracker.ProcessOnMoonQuit();
                Tracker.UpdatePlanetExpeditionData(__instance.currentLevel);
            } catch (System.Exception e) {
                LogError(e, nameof(OnMoonExit));
            }
        }

        static void OnPlayerDeath() {
            try {

            } catch (System.Exception e) {
                LogError(e, nameof(OnPlayerDeath));
            }
        }

        private static void LogError(System.Exception e, string occurMethod) {
            StatTrackerMod.Logger.LogError($"An error has occurred in method {occurMethod}: {e.Message}");
        }
    }
}
