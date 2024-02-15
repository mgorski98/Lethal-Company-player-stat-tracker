using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace LethalCompanyStatTracker {
    [HarmonyPatch(typeof(Terminal))]
    internal class BoughtItemsStorePatch {

        private static FieldInfo totalCostOfItems_FieldInfo;
        private static List<int> BoughtItemsSnapshot = new List<int>();

        [HarmonyPatch("LoadNewNodeIfAffordable")]
        [HarmonyPrefix]
        static void SnapshotBeforeBuyingNewItems(Terminal __instance, TerminalNode node) {
            if (node.buyItemIndex == -1)
                return;
            BoughtItemsSnapshot.Clear();
            BoughtItemsSnapshot.AddRange(__instance.orderedItemsFromTerminal);
        }

        //this should only work when you're buying
        [HarmonyPatch("LoadNewNodeIfAffordable")]
        [HarmonyPostfix]
        static void StoreBoughtItems(Terminal __instance, TerminalNode node) {
            FetchTotalCostInfoIfNull();
            if (node.shipUnlockableID == -1) {
                StatisticsTracker.Instance.StoreShopBoughtItems(__instance.orderedItemsFromTerminal.Skip(BoughtItemsSnapshot.Count).ToArray(), __instance);
                BoughtItemsSnapshot.Clear();
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "UnlockShipObject")]
        [HarmonyPrefix]
        static void StoreUnlockableItems(int unlockableID) {
            if (unlockableID == -1)
                return;
            FetchTotalCostInfoIfNull();
            var item = StartOfRound.Instance.unlockablesList.unlockables[unlockableID];
            if (item.alreadyUnlocked || item.hasBeenUnlockedByPlayer) {
                return;
            }
            var terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            var data = StatisticsTracker.Instance.cumulativeData.allBoughtItems[item.unlockableName];
            var totalCost = (int)totalCostOfItems_FieldInfo.GetValue(terminal);
            data.Count++;
            data.TotalPrice += totalCost;
            StatTrackerMod.Logger.LogMessage($"Stored unlockable named {item.unlockableName}, costing {totalCost}");
        }

        private static FieldInfo ItemsToDeliver_FieldInfo;

        [HarmonyPatch(typeof(ItemDropship), "Start")]
        [HarmonyPostfix]
        static void FetchBoughtItemsFromItemDropship(ItemDropship __instance) {
            if (__instance.IsServer)
                return;
            if (ItemsToDeliver_FieldInfo == null)
                ItemsToDeliver_FieldInfo = typeof(ItemDropship).GetField("itemsToDeliver", BindingFlags.Instance | BindingFlags.NonPublic);
            var itemsToDeliver_indices = ItemsToDeliver_FieldInfo.GetValue(__instance) as List<int>;
            var terminal = UnityEngine.Object.FindObjectOfType(typeof(Terminal));
            StatisticsTracker.Instance.StoreShopBoughtItems(itemsToDeliver_indices.ToArray(), (Terminal)terminal);
        }

        [HarmonyPatch(typeof(Terminal), "SyncGroupCreditsClientRpc")]
        [HarmonyPrefix]
        static void UpdateCreditsSpent(Terminal __instance, int newGroupCredits) {
            var diff = __instance.groupCredits - newGroupCredits;
            if (diff < 0) {
                //money was spent
                var value = UnityEngine.Mathf.Abs(diff);
                StatisticsTracker.Instance.cumulativeData.totalMoneySpent += value;
                StatTrackerMod.Logger.LogMessage($"Spent {value} credits");
            }
        }

        static void FetchTotalCostInfoIfNull() {
            if (totalCostOfItems_FieldInfo == null) {
                totalCostOfItems_FieldInfo = typeof(Terminal).GetField("totalCostOfItems", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }
    }
}
