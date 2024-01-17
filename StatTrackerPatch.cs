using HarmonyLib;
using UnityEngine;

namespace LethalCompanyStatTracker {
    internal class StatTrackerPatch {
        private static StatisticsTracker Tracker => StatisticsTracker.Instance;

        [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void OnMoonStart() {
            Tracker.SnapshotCollectedItemsOnMoonStart();
        }

        [HarmonyPatch(typeof(StartMatchLever), "EndGame")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void OnMoonExit_Initial() {
            Tracker.InitialProcessOnQuit();
            Tracker.ShowCollectedItemsDialog();
        }

        [HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void OnMoonExit_End() {
            //this method checks items dropped on ship AND player's inventory items and stores them in stats
            Tracker.ProcessOnMoonQuit();
            Tracker.UpdatePlanetExpeditionData(RoundManager.Instance.currentLevel);
        }

        //this method adds every item dropped in the ship to the storage
        //todo: make sure this doesn't fire on the Company building
        [HarmonyPatch(typeof(HUDManager), "AddNewScrapFoundToDisplay")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void OnScrapAdded(GrabbableObject GObject) {
            var data = Tracker.cumulativeData.allCollectedItems[GObject.itemProperties.itemName];
            data.Count++;
            data.TotalPrice += GObject.scrapValue;

            Tracker.currentlyCollected.Add(GObject);
            StatTrackerMod.Logger.LogMessage($"Collected {GObject.itemProperties.itemName}, worth {GObject.scrapValue}");
        }
    }
}
