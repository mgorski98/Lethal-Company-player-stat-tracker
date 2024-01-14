using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using GameNetcodeStuff;

namespace LethalCompanyStatTracker {
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerSpawnStatTrackerPatch {
        [HarmonyPatch("InstantiateBloodPooledObjects")]
        [HarmonyPostfix]
        static void SpawnStatTrackOnPlayer(ref PlayerControllerB __instance) {
            if (__instance == null)
                return;
            if (!__instance.IsOwner)
                return;

            if (!__instance.TryGetComponent(out StatisticsTracker _tracker))
                __instance.gameObject.AddComponent<StatisticsTracker>();

            StatTrackerMod.Logger.LogMessage($"Spawned stat tracker on player {__instance.playerUsername}");
        }
    }
}
