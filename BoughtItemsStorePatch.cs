using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace LethalCompanyStatTracker {
    [HarmonyPatch(typeof(Terminal))]
    internal class BoughtItemsStorePatch {
        [HarmonyPatch("SyncBoughtItemsWithServer")]
        [HarmonyBefore]
        static void StoreBoughtItems(Terminal __instance, int[] boughtItems) {
            foreach (var itemIndex in boughtItems) {
                var item = __instance.buyableItemsList[itemIndex];
                var name = item.itemName;
                if (!StatisticsTracker.Instance.allBoughtItems.TryGetValue(name, out var data)) {
                    var newData = new StatisticsTracker.ItemData() { TotalPrice = -1, Count = 1, ItemName = name };
                    StatisticsTracker.Instance.allBoughtItems[name] = newData;
                } else {
                    data.Count++;
                }
            }
        }
    }
}
