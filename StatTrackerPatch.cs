using HarmonyLib;
using UnityEngine;

namespace LethalCompanyStatTracker {
    [HarmonyPatch(typeof(RoundManager))]
    internal class StatTrackerPatch {
        private static StatisticsTracker Tracker;

        [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
        [HarmonyPostfix]
        static void OnMoonStart() {
            if (Tracker == null) {
                Tracker = GameObject.FindObjectOfType<StatisticsTracker>();
            }
            Tracker.SnapshotCollectedItemsOnMoonStart();
        }

        [HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
        [HarmonyPrefix]
        static void OnMoonExit() {
            Tracker.ProcessOnMoonQuit();
        }
    }
}
