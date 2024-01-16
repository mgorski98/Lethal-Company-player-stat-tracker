using HarmonyLib;
using GameNetcodeStuff;

namespace LethalCompanyStatTracker {
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerSpawnStatTrackerPatch {
        [HarmonyPatch("InstantiateBloodPooledObjects")]
        [HarmonyPostfix]
        static void SpawnStatTrackOnPlayer(ref PlayerControllerB __instance) {
            if (__instance != GameNetworkManager.Instance.localPlayerController)
                return;
            if (!__instance.TryGetComponent(out StatisticsTracker _tracker))
                __instance.gameObject.AddComponent<StatisticsTracker>();

            StatTrackerMod.Logger.LogMessage($"Spawned stat tracker on player {__instance.playerUsername}");
        }
    }
}
