using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalCompanyStatTracker {
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManagerPatch {
        static void SpawnShowStatsButton() {

        }

        static void ShowStats() {

        }

        static void ClearStats() {

        }

        static void ShowConfirmationDialog() {

        }
    }
}
