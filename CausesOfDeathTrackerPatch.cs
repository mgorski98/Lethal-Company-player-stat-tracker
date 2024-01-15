using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace LethalCompanyStatTracker {
    internal class CausesOfDeathTrackerPatch {
        public static class DeathCauseConstants {
            const string JESTER = "Jester";
            const string COILHEAD = "Coil-Head";
            const string BLOB = "Hydrogere";
            const string HOARDING_BUG = "Hoarding bug";
            const string THUMPER = "Thumper";
            const string DOG = "Eyeless dog";
            const string GIANT = "Forest giant";
            const string SNARE_FLEA = "Snare flea";
            const string GRAVITY = "Falling";
            const string QUICKSAND = "Quicksand";
            const string WORM = "Earth leviathan";
            const string GIRL = "Ghost girl";
            const string BABOON = "Baboon hawk";
            const string MASKED = "Masked";
            const string SPORE_DOG = "Spore lizard";
            const string BRACKEN = "Bracken";
            const string NUTCRACKER = "Nutcracker";
            const string SPIDER = "Bunker spider";
            const string NOT_THE_BEES = "Circuit bees";
            const string MINE = "Landmine";
            const string TURRET = "Turret";
            const string EXT_LADDER = "Extension ladder";
            const string COMPANY_MONSTER = "Jeb (AKA Company Monster)";
        }

        [HarmonyPatch(typeof(JesterAI), "")]
        [HarmonyPostfix]
        static void JesterAIPatch(JesterAI __instance) {

        }

        [HarmonyPatch(typeof(SpringManAI), "")]
        [HarmonyPostfix]
        static void CoilheadAIPatch(SpringManAI __instance) {

        }

        [HarmonyPatch(typeof(CrawlerAI), "")]
        [HarmonyPostfix]
        static void ThumperAIPatch(CrawlerAI __instance) {

        }

        [HarmonyPatch(typeof(MouthDogAI), "")]
        [HarmonyPostfix]
        static void DogAIPatch(MouthDogAI __instance) {

        }

        [HarmonyPatch(typeof(BlobAI), "")]
        [HarmonyPostfix]
        static void BlobAIPatch(BlobAI __instance) {

        }

        [HarmonyPatch(typeof(PufferAI), "")]
        [HarmonyPostfix]
        static void SporeLizardAIPatch(PufferAI __instance) {

        }

        [HarmonyPatch(typeof(FlowermanAI), "")]
        [HarmonyPostfix]
        static void BrackenAIPatch(FlowermanAI __instance) {

        }

        [HarmonyPatch(typeof(DressGirlAI), "")]
        [HarmonyPostfix]
        static void GirlAIPatch(DressGirlAI __instance) {

        }

        [HarmonyPatch(typeof(NutcrackerEnemyAI), "")]
        [HarmonyPostfix]
        static void NutcrackerAIPatch(NutcrackerEnemyAI __instance) {

        }

        [HarmonyPatch(typeof(SandSpiderAI), "")]
        [HarmonyPostfix]
        static void SpiderAIPatch(SandSpiderAI __instance) {

        }

        [HarmonyPatch(typeof(HoarderBugAI), "")]
        [HarmonyPostfix]
        static void LootbugAIPatch(HoarderBugAI __instance) {

        }

        [HarmonyPatch(typeof(ForestGiantAI), "")]
        [HarmonyPostfix]
        static void ForestGiantAIPatch(ForestGiantAI __instance) {

        }

        [HarmonyPatch(typeof(BaboonBirdAI), "")]
        [HarmonyPostfix]
        static void BaboonHawkAIPatch(BaboonBirdAI __instance) {

        }

        [HarmonyPatch(typeof(RedLocustBees), "")]
        [HarmonyPostfix]
        static void CircuitBeesAIPatch(RedLocustBees __instance) {

        }

        [HarmonyPatch(typeof(MaskedPlayerEnemy), "")]
        [HarmonyPostfix]
        static void MaskedAIPatch(MaskedPlayerEnemy __instance) {

        }

        [HarmonyPatch(typeof(CentipedeAI), "")]
        [HarmonyPostfix]
        static void SnareFleaAIPatch(CentipedeAI __instance) {

        }

        [HarmonyPatch(typeof(SandWormAI), "")]
        [HarmonyPostfix]
        static void EarthWormAIPatch(SandWormAI __instance) {

        }

        [HarmonyPostfix]
        static void QuicksandDeathPatch() {

        }

        [HarmonyPostfix]
        static void DeathByGravityPatch() {

        }

        [HarmonyPostfix]
        static void DeathByMinePatch() {

        }

        [HarmonyPostfix]
        static void DeathByTurretPatch() {

        }

        [HarmonyPostfix]
        static void DeathByFallingLadderPatch() {

        }

        private static void LogDeathCause(string deathCause, bool isEnemy) {
            StatTrackerMod.Logger.LogMessage($"You were killed by {deathCause}{(isEnemy ? " enemy" : "")}");
        }
    }
}
