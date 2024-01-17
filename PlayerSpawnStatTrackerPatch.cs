using HarmonyLib;
using GameNetcodeStuff;
using Unity.Netcode;

namespace LethalCompanyStatTracker {
    [HarmonyPatch(typeof(StartOfRound))]
    internal class PlayerSpawnStatTrackerPatch {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void SpawnStatTrackOnPlayer(ref StartOfRound __instance) {
            if (!__instance.TryGetComponent(out StatisticsTracker _tracker)) {
                StatTrackerMod.Logger.LogMessage($"Spawned stat tracker");
                __instance.gameObject.AddComponent<StatisticsTracker>();
            }
        }
    }
}
