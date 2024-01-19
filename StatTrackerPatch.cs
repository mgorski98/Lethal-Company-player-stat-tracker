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
            //also dont store these things on quitting the company building
            if (RoundManager.Instance.currentLevel.PlanetName == "71 Gordion")
                return;
            Tracker.ProcessOnMoonQuit();
            Tracker.UpdatePlanetExpeditionData(RoundManager.Instance.currentLevel);
            Tracker.UpdateHighestQuotaReached();
        }

        //this method adds every item dropped in the ship to the storage
        //todo: make sure this doesn't fire on the Company building
        [HarmonyPatch(typeof(HUDManager), "AddNewScrapFoundToDisplay")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void OnScrapAdded(GrabbableObject GObject) {
            //ignore this method on company building
            if (GObject is RagdollGrabbableObject) {
                return;
            }
            if (RoundManager.Instance.currentLevel.PlanetName == "71 Gordion") {
                return;
            }
            var data = Tracker.cumulativeData.allCollectedItems[GObject.itemProperties.itemName];
            data.Count++;
            data.TotalPrice += GObject.scrapValue;

            Tracker.currentlyCollected.Add(GObject);
            StatTrackerMod.Logger.LogMessage($"Collected {GObject.itemProperties.itemName}, worth {GObject.scrapValue}");
        }

        [HarmonyWrapSafe]
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StartOfRound), "FirePlayersAfterDeadlineClientRpc")]
        static void OnEndOfGame() {
            StatisticsTracker.Instance.HandleEndOfGame();
        }

        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
        static void OnEndOfRound(int bodiesInsured) {
            StatisticsTracker.Instance.cumulativeData.bodiesInsured += bodiesInsured;
            StatTrackerMod.Logger.LogMessage($"Storing {bodiesInsured} collected bodies");
        }

        [HarmonyWrapSafe]
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Terminal), "SyncGroupCreditsClientRpc")]
        static void OnCreditsSpent(Terminal __instance, int newGroupCredits) {
            StatisticsTracker.Instance.UpdateCreditsSpent(__instance.groupCredits, newGroupCredits);
        }
    }
}
