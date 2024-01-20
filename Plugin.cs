using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalCompanyStatTracker.TerminalStuff;
using UnityEngine;
using System.Reflection;

namespace LethalCompanyStatTracker
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("atomic.terminalapi")]
    public class StatTrackerMod : BaseUnityPlugin
    {
        public const string ModGUID = "DingDingDev.StatTracker";
        public const string ModName = "Stat tracker";
        public const string ModVersion = "1.0";

        public static new ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(ModGUID);

        private readonly Harmony HarmonyClient = new Harmony(ModGUID);

        private System.Type[] Patches = new System.Type[] {
            typeof(StatTrackerPatch),
            typeof(PlayerSpawnStatTrackerPatch),
            typeof(CausesOfDeathTrackerPatch),
            typeof(EnemyKillsTrackerPatch),
            typeof(DepositItemsDeskPatch),
            typeof(TerminalCommandsPatch),
            typeof(BoughtItemsStorePatch)
        };

        void Awake() {
            foreach (var patchType in Patches)
                HarmonyClient.PatchAll(patchType);

            Logger.LogMessage("Initialized correctly!");
        }
    }
}
