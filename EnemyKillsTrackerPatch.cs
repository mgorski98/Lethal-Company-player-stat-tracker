using GameNetcodeStuff;
using HarmonyLib;

namespace LethalCompanyStatTracker {
    internal class EnemyKillsTrackerPatch {

        public static class Enemies {
            public const string BRACKEN = "Bracken";
            public const string THUMPER = "Thumper";
            public const string DOG = "Eyeless dog";
            public const string SPORE_DOG = "Spore lizard";
            public const string HOARDING_BUG = "Hoarding bug";
            public const string NUTCRACKER = "Nutcracker";
            public const string FLEA = "Snare flea";
            public const string SPIDER = "Bunker spider";
            public const string MASKED = "Masked";
            public const string BABOON = "Baboon Hawk";
            public const string A_FRIEND = "Other players :(";
        }

        private const string TARGET_METHOD = "HitEnemy";

        [HarmonyPatch(typeof(FlowermanAI), TARGET_METHOD)]
        [HarmonyPostfix]
        static void OnBrackenKill(FlowermanAI __instance, PlayerControllerB playerWhoHit) {
            TrackEnemyKilled(__instance, Enemies.BRACKEN, playerWhoHit);
        }

        [HarmonyPatch(typeof(PufferAI), TARGET_METHOD)]
        [HarmonyPostfix]
        static void OnSporeLizardKill(PufferAI __instance, PlayerControllerB playerWhoHit) {
            TrackEnemyKilled(__instance, Enemies.SPORE_DOG, playerWhoHit);
        }

        [HarmonyPatch(typeof(CrawlerAI), TARGET_METHOD)]
        [HarmonyPostfix]
        static void OnThumperKill(CrawlerAI __instance, PlayerControllerB playerWhoHit) {
            TrackEnemyKilled(__instance, Enemies.THUMPER, playerWhoHit);
        }

        [HarmonyPatch(typeof(MouthDogAI), TARGET_METHOD)]
        [HarmonyPostfix]
        static void OnDogKill(MouthDogAI __instance, PlayerControllerB playerWhoHit) {
            TrackEnemyKilled(__instance, Enemies.DOG, playerWhoHit);
        }

        [HarmonyPatch(typeof(CentipedeAI), TARGET_METHOD)]
        [HarmonyPostfix]
        static void OnFleaKill(CentipedeAI __instance, PlayerControllerB playerWhoHit) {
            TrackEnemyKilled(__instance, Enemies.FLEA, playerWhoHit);
        }

        [HarmonyPatch(typeof(SandSpiderAI), TARGET_METHOD)]
        [HarmonyPostfix]
        static void OnSpiderKill(SandSpiderAI __instance, PlayerControllerB playerWhoHit) {
            TrackEnemyKilled(__instance, Enemies.SPIDER, playerWhoHit);
        }

        [HarmonyPatch(typeof(HoarderBugAI), TARGET_METHOD)]
        [HarmonyPostfix]
        static void OnHoardingBugKill(HoarderBugAI __instance, PlayerControllerB playerWhoHit) {
            TrackEnemyKilled(__instance, Enemies.HOARDING_BUG, playerWhoHit);
        }

        [HarmonyPatch(typeof(BaboonBirdAI), TARGET_METHOD)]
        [HarmonyPostfix]
        static void OnBaboonHawkKill(BaboonBirdAI __instance, PlayerControllerB playerWhoHit) {
            TrackEnemyKilled(__instance, Enemies.BABOON, playerWhoHit);
        }

        [HarmonyPatch(typeof(MaskedPlayerEnemy), TARGET_METHOD)]
        [HarmonyPostfix]
        static void OnMaskedKill(MaskedPlayerEnemy __instance, PlayerControllerB playerWhoHit) {
            TrackEnemyKilled(__instance, Enemies.MASKED, playerWhoHit);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Hit")]
        [HarmonyPostfix]
        static void OnFriendlyFireKill(ref PlayerControllerB __instance, PlayerControllerB playerWhoHit) {
            if (!__instance.IsOwner && playerWhoHit.IsOwner && __instance.isPlayerDead) {
                StatisticsTracker.Instance.OnEnemyKilled(Enemies.A_FRIEND, playerWhoHit.playerClientId);
            }
        }

        [HarmonyPatch(typeof(NutcrackerEnemyAI), TARGET_METHOD)]
        [HarmonyPostfix]
        static void OnNutcrackerKill(NutcrackerEnemyAI __instance, PlayerControllerB playerWhoHit) {
            TrackEnemyKilled(__instance, Enemies.NUTCRACKER, playerWhoHit);
        }

        static void TrackEnemyKilled(EnemyAI enemyScript, string enemyName, PlayerControllerB player) {
            if (enemyScript.enemyHP <= 0) {
                StatisticsTracker.Instance.OnEnemyKilled(enemyName, player.playerClientId);
            }
        }
    }
}
