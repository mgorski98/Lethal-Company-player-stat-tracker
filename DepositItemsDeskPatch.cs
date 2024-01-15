using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
