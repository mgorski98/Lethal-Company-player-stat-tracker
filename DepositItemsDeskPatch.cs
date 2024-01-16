using HarmonyLib;

namespace LethalCompanyStatTracker {
    [HarmonyPatch(typeof(DepositItemsDesk))]
    internal class DepositItemsDeskPatch {
        [HarmonyPrefix]
        [HarmonyPatch("delayedAcceptanceOfItems")]
        static void StoreItemsSold(GrabbableObject[] objectsOnDesk) {
            StatisticsTracker.Instance.StoreSoldItems(objectsOnDesk);
        }
    }
}
