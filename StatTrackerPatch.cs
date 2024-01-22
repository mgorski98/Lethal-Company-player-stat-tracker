using GameNetcodeStuff;
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

        [HarmonyPatch(typeof(HUDManager), "DisplayDaysLeft")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void OnAllPlayersDied() {
            Tracker.ShowStreakLostInfo(allPlayersDead);
        }

        static bool allPlayersDead = false;

        [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
        [HarmonyPrefix]
        static void AllPlayersDied() {
            allPlayersDead = StartOfRound.Instance.allPlayersDead;
        }

        [HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void OnMoonExit_End() {
            //this method checks items dropped on ship AND player's inventory items and stores them in stats
            //also dont store these things on quitting the company building
            if (RoundManager.Instance.currentLevel.IsCompanyBuilding())
                return;
            Tracker.ProcessOnMoonQuit();
            Tracker.UpdatePlanetExpeditionData(RoundManager.Instance.currentLevel);
            Tracker.UpdateHighestQuotaReached();
            Tracker.StorePlayerDeaths();
        }

        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(TimeOfDay), "SyncNewProfitQuotaClientRpc")]
        static void OnHighestQuotaChanged(int newProfitQuota) {
            var highestQuota = Tracker.cumulativeData.highestQuotaReached;
            if (newProfitQuota > highestQuota) {
                Tracker.cumulativeData.highestQuotaReached = newProfitQuota;
                Tracker.cumulativeData.totalTimesQuotaFulfilled++;
                StatTrackerMod.Logger.LogMessage($"Setting new highest quota: {newProfitQuota}");
            }
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
            if (RoundManager.Instance.currentLevel.IsCompanyBuilding()) {
                return;
            }
            var data = Tracker.cumulativeData.allCollectedItems[GObject.itemProperties.itemName];
            data.Count++;
            data.TotalPrice += GObject.scrapValue;

            Tracker.currentlyCollected.Add(GObject);
            StatTrackerMod.Logger.LogMessage($"Collected {GObject.itemProperties.itemName}, worth {GObject.scrapValue}");
        }

        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
        static void OnEndOfRound(int bodiesInsured) {
            Tracker.cumulativeData.bodiesInsured += bodiesInsured;
            StatTrackerMod.Logger.LogMessage($"Storing {bodiesInsured} collected bodies");
        }

        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayer")]
        [HarmonyPrefix]
        static void OnDamagePlayer(int damageNumber) {
            Tracker.cumulativeData.totalDamage += damageNumber;
            StatTrackerMod.Logger.LogMessage($"Player was damaged! updating stats: {damageNumber}");
        }

        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(PlayerControllerB), "PlayerJump")]
        [HarmonyPrefix]
        static void OnPlayerJump() {
            Tracker.cumulativeData.totalJumps++;
            StatTrackerMod.Logger.LogMessage($"Player jumped! updating stats");
        }

        [HarmonyWrapSafe]
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), "UpdatePlayerPositionClientRpc")]
        static void OnPlayerMoved() {
            Tracker.cumulativeData.totalSteps++;
            //StatTrackerMod.Logger.LogMessage($"Player moved! updating stats");
        }

        //[HarmonyWrapSafe]
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(Terminal), "SyncGroupCreditsClientRpc")]
        //static void OnCreditsSpent(Terminal __instance, int newGroupCredits) {
        //    StatisticsTracker.Instance.UpdateCreditsSpent(__instance.groupCredits, newGroupCredits);
        //}
    }
}
