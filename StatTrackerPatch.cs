using HarmonyLib;
using UnityEngine;

namespace LethalCompanyStatTracker {
    internal class StatTrackerPatch {
        private static StatisticsTracker Tracker;

        [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void OnMoonStart() {
            if (Tracker == null) {
                Tracker = GameObject.FindObjectOfType<StatisticsTracker>();
            }
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
            if (!Tracker.allCollectedItems.TryGetValue(GObject.itemProperties.itemName, out var data)) {
                Tracker.allCollectedItems[GObject.itemProperties.itemName] = new StatisticsTracker.ItemData() {
                    Count = 1,
                    TotalPrice = GObject.scrapValue,
                    ItemName = GObject.itemProperties.itemName
                };
            } else {
                data.Count++;
                data.TotalPrice += GObject.scrapValue;
            }

            Tracker.currentlyCollected.Add(GObject);
            StatTrackerMod.Logger.LogMessage($"Collected {GObject.itemProperties.itemName}, worth {GObject.scrapValue}");
        }
    }
}
