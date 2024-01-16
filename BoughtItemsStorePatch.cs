using HarmonyLib;

namespace LethalCompanyStatTracker {
    [HarmonyPatch(typeof(Terminal))]
    internal class BoughtItemsStorePatch {
        [HarmonyPatch("SyncBoughtItemsWithServer")]
        [HarmonyPrefix]
        static void StoreBoughtItems(Terminal __instance, int[] boughtItems) {
            StatisticsTracker.Instance.StoreShopBoughtItems(boughtItems, __instance);
        }
    }
}
