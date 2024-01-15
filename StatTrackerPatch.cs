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

        [HarmonyPatch(typeof(StartMatchLever), "EndGame")]
        [HarmonyPostfix]
        static void OnMoonExit_Initial() {
            try {
                Tracker.InitialProcessOnQuit();
                Tracker.ShowCollectedItemsDialog();
            } catch (System.Exception e) {
                LogError(e, nameof(OnMoonExit_Initial));
            }
        }

        [HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
        [HarmonyPostfix]
        static void OnMoonExit_End() {
            try {
                Tracker.ProcessOnMoonQuit();
                Tracker.UpdatePlanetExpeditionData(RoundManager.Instance.currentLevel);
            } catch (System.Exception e) {
                LogError(e, nameof(OnMoonExit_End));
            }
        }

        private static void LogError(System.Exception e, string occurMethod) {
            StatTrackerMod.Logger.LogError($"An error has occurred in method {occurMethod}: {e.Message}\nTrace: {e.StackTrace}");
        }
    }
}
