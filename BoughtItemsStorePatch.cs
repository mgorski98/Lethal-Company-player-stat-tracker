using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace LethalCompanyStatTracker {
    [HarmonyPatch(typeof(Terminal))]
    internal class BoughtItemsStorePatch {

        private static FieldInfo totalCostOfItems_FieldInfo;

        private static List<int> BoughtItemsSnapshot = new List<int>();

        //this should only work when you're buying
        [HarmonyPatch("BuyItemsServerRpc")]
        [HarmonyPostfix]
        static void StoreBoughtItems(Terminal __instance, int[] boughtItems) {
            FetchTotalCostInfoIfNull();
            int totalCostOfItems = (int)totalCostOfItems_FieldInfo.GetValue(__instance);
            StatisticsTracker.Instance.StoreShopBoughtItems(boughtItems, __instance, totalCostOfItems);
        }

        [HarmonyPostfix]
        [HarmonyPatch("LoadNewNodeIfAffordable")]
        static void StoreBoughtItems_ServerSide(Terminal __instance) {

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
