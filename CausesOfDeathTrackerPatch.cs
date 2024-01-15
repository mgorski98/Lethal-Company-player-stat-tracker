using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

/*TODO: PASS KILLED PLAYER ID TO ON PLAYER DEATH METHOD SO THAT IT CAN BE VERIFIED BY THE METHOD*/

namespace LethalCompanyStatTracker {
    internal class CausesOfDeathTrackerPatch {
        public static class DeathCauseConstants {
            public const string JESTER = "Jester";
            public const string COILHEAD = "Coil-Head";
            public const string BLOB = "Hydrogere";
            public const string HOARDING_BUG = "Hoarding bug";
            public const string THUMPER = "Thumper";
            public const string DOG = "Eyeless dog";
            public const string GIANT = "Forest giant";
            public const string SNARE_FLEA = "Snare flea";
            public const string GRAVITY = "Falling";
            public const string WORM = "Earth leviathan";
            public const string GIRL = "Ghost girl";
            public const string BABOON = "Baboon hawk";
            public const string MASKED = "Masked";
            public const string SPORE_DOG = "Spore lizard";
            public const string BRACKEN = "Bracken";
            public const string NUTCRACKER = "Nutcracker";
            public const string SPIDER = "Bunker spider";
            public const string NOT_THE_BEES = "Circuit bees";
            public const string MINE = "Landmine";
            public const string TURRET = "Turret";
            public const string EXT_LADDER = "Extension ladder";
            public const string COMPANY_MONSTER = "Jeb (AKA Company Monster)";
            public const string DROWNING = "Drowning & Quicksand";
            public const string ABANDON = "Abandonment";
            public const string FRIENDLY_FIRE = "Friendly fire";
        }

        [HarmonyPatch(typeof(JesterAI), "killPlayerAnimation")]
        [HarmonyPostfix]
        static void JesterAIPatch(int playerId) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.JESTER, StartOfRound.Instance.allPlayerScripts[playerId].playerClientId);
        }

        [HarmonyPatch(typeof(SpringManAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        static void CoilheadAIPatch(Collider other) {
            var playerController = other.GetComponent<PlayerControllerB>();
            if (playerController == null)
                return;
            var id = playerController.playerClientId;

            //just in case someone doesn't sync properly
            if ((playerController.health <= 0 || playerController.isPlayerDead) && playerController.IsOwner) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.COILHEAD, id);
            }
        }

        [HarmonyPatch(typeof(CrawlerAI), "EatPlayerBodyAnimation")]
        [HarmonyPostfix]
        static void ThumperAIPatch(int playerId) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.THUMPER, StartOfRound.Instance.allPlayerScripts[playerId].playerClientId);
        }

        [HarmonyPatch(typeof(MouthDogAI), "KillPlayer")]
        [HarmonyPostfix]
        static void DogAIPatch(int playerId) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.DOG, StartOfRound.Instance.allPlayerScripts[playerId].playerClientId);
        }

        [HarmonyPatch(typeof(BlobAI), "eatPlayerBody")]
        [HarmonyPostfix]
        static void BlobAIPatch(int playerKilled) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.BLOB, StartOfRound.Instance.allPlayerScripts[playerKilled].playerClientId);
        }

        [HarmonyPatch(typeof(PufferAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        static void SporeLizardAIPatch(Collider other) {
            var playerController = other.GetComponent<PlayerControllerB>();
            if (playerController == null)
                return;
            var id = playerController.playerClientId;

            if ((playerController.health <= 0 || playerController.isPlayerDead) && playerController.IsOwner) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.SPORE_DOG, id);
            }
        }

        [HarmonyPatch(typeof(FlowermanAI), "killAnimation")]
        [HarmonyPrefix]
        static void BrackenAIPatch(FlowermanAI __instance) {
            if (__instance.inSpecialAnimationWithPlayer == null)
                return;
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.BRACKEN, __instance.inSpecialAnimationWithPlayer.playerClientId);
        }

        [HarmonyPatch(typeof(DressGirlAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        static void GirlAIPatch(DressGirlAI __instance, Collider other) {
            var controller = other.GetComponent<PlayerControllerB>();
            if (controller == null)
                return;

            if (controller == __instance.hauntingPlayer && controller.isPlayerDead && controller.IsOwner) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.GIRL, controller.playerClientId);
            }
        }

        [HarmonyPatch(typeof(NutcrackerEnemyAI), "LegKickPlayer")]
        [HarmonyPostfix]
        static void NutcrackerKickAIPatch(int playerId) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.NUTCRACKER, StartOfRound.Instance.allPlayerScripts[playerId].playerClientId);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Hit")]
        [HarmonyPostfix]
        static void NutcrackerShotgunAnFriendlyFirePatch(ref PlayerControllerB __instance, PlayerControllerB playerWhoHit) {
            if (__instance == GameNetworkManager.Instance.localPlayerController) {
                var cause = playerWhoHit == null ? DeathCauseConstants.NUTCRACKER : DeathCauseConstants.FRIENDLY_FIRE;
                StatisticsTracker.Instance.OnPlayerDeath(cause, __instance.playerClientId);
            }
        }

        [HarmonyPatch(typeof(SandSpiderAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        static void SpiderAIPatch(Collider other) {
            var controller = other.GetComponent<PlayerControllerB>();
            if (controller == null)
                return;

            if ((controller.isPlayerDead || controller.health <= 0) && controller.IsOwner) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.SPIDER, controller.playerClientId);
            }
        }

        [HarmonyPatch(typeof(HoarderBugAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        static void LootbugAIPatch(Collider other) {
            var controller = other.GetComponent<PlayerControllerB>();
            if (controller == null)
                return;

            if (controller.health <= 0 || controller.isPlayerDead && controller.IsOwner) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.HOARDING_BUG, controller.playerClientId);
            }
        }

        [HarmonyPatch(typeof(BaboonBirdAI), "killPlayerAnimation")]
        [HarmonyPrefix]
        static void BaboonHawkAIPatch(int playerObject) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.BABOON, StartOfRound.Instance.allPlayerScripts[playerObject].playerClientId);
        }

        [HarmonyPatch(typeof(RedLocustBees), "BeesKillPlayer")]
        [HarmonyPostfix]
        static void CircuitBeesAIPatch(PlayerControllerB killedPlayer) {
            var id = killedPlayer?.playerClientId ?? 0;
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.NOT_THE_BEES, id);
        }

        [HarmonyPatch(typeof(MaskedPlayerEnemy), "killAnimation")]
        [HarmonyPostfix]
        static void MaskedAIPatch(MaskedPlayerEnemy __instance) {
            var player = __instance.inSpecialAnimationWithPlayer;
            if (player == null)
                return;

            if (player.isPlayerDead && player.IsOwner) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.MASKED, player.playerClientId);
            }
        }

        [HarmonyPatch(typeof(CentipedeAI), "UnclingFromPlayer")]
        [HarmonyPostfix]
        static void SnareFleaAIPatch(PlayerControllerB playerBeingKilled, bool playerDead) {
            if (playerDead) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.SNARE_FLEA, playerBeingKilled.playerClientId);
            }
        }

        [HarmonyPatch(typeof(SandWormAI), "EatPlayer")]
        [HarmonyPostfix]
        static void EarthWormAIPatch(PlayerControllerB playerScript) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.WORM, playerScript.playerClientId);
        }

        [HarmonyPatch(typeof(DepositItemsDesk), "AnimationGrabPlayer")]
        [HarmonyPostfix]
        static void CompanyMonsterDeath(int playerID) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.COMPANY_MONSTER, StartOfRound.Instance.allPlayerScripts[playerID].playerClientId);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
        [HarmonyPostfix]
        static void NonEnemiesDeathPatch(PlayerControllerB __instance, CauseOfDeath causeOfDeath) {
            var id = __instance.playerClientId;
            if (__instance.isPlayerDead && __instance.IsOwner) {
                switch (causeOfDeath) {
                    case CauseOfDeath.Blast: {
                            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.MINE, id);
                            LogDeathCause(DeathCauseConstants.MINE, false);
                            break;
                    }
                    case CauseOfDeath.Drowning: {
                            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.DROWNING, id);
                            LogDeathCause(DeathCauseConstants.DROWNING, false);
                            break;
                        }

                    case CauseOfDeath.Gunshots: {
                            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.TURRET, id);
                            LogDeathCause(DeathCauseConstants.TURRET, false);
                            break;
                        }
                    case CauseOfDeath.Abandoned: {
                            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.ABANDON, id);
                            LogDeathCause(DeathCauseConstants.ABANDON, false);
                            break;
                        }
                    case CauseOfDeath.Gravity: {
                            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.GRAVITY, id);
                            LogDeathCause(DeathCauseConstants.GRAVITY, false);
                            break;
                        }
                    case CauseOfDeath.Crushing: {
                            //zobaczymy czy działa i czy drabina też się nie liczy do tego
                            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.GIANT, id);
                            LogDeathCause(DeathCauseConstants.GIANT, true);
                            break;
                        }
                    //todo: check which kind of death cause is falling extension ladder
                    default:
                        break;
                }
            }
        }

        private static void LogDeathCause(string deathCause, bool isEnemy) {
            StatTrackerMod.Logger.LogMessage($"You were killed by {deathCause}{(isEnemy ? " enemy" : "")}");
        }
    }
}
