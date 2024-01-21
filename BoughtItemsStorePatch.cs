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

        [HarmonyPatch("LoadNewNodeIfAffordable")]
        [HarmonyPostfix]
        static void StoreBoughtItems(Terminal __instance, TerminalNode node) {
            if (node.buyItemIndex == -1)
                return;

            FetchTotalCostInfoIfNull();
            int totalCostOfItems = (int)totalCostOfItems_FieldInfo.GetValue(__instance);
            if (node.shipUnlockableID != -1) {//it works, im scared to touch this :D
            } else {
                var newItems = __instance.orderedItemsFromTerminal.Skip(BoughtItemsSnapshot.Count).ToArray();
                StatisticsTracker.Instance.StoreShopBoughtItems(newItems, __instance, totalCostOfItems);
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

        static void FetchTotalCostInfoIfNull() {
            if (totalCostOfItems_FieldInfo == null) {
                totalCostOfItems_FieldInfo = typeof(Terminal).GetField("totalCostOfItems", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }
    }
}
