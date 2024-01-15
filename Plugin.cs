using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using GameNetcodeStuff;

namespace LethalCompanyStatTracker
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class StatTrackerMod : BaseUnityPlugin
    {
        public const string ModGUID = "DingDingDev.StatTracker";
        public const string ModName = "Stat tracker";
        public const string ModVersion = "1.0";

        public static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(ModGUID);

        private readonly Harmony HarmonyClient = new Harmony(ModGUID);

        void Awake() {
            HarmonyClient.PatchAll(typeof(StatTrackerPatch));
            HarmonyClient.PatchAll(typeof(PlayerSpawnStatTrackerPatch));
            HarmonyClient.PatchAll(typeof(CausesOfDeathTrackerPatch));

            Logger.LogMessage("Initialized correctly!");
        }
    }
}
