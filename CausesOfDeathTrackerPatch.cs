using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using System.Collections;

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
            public const string MINE = "Landmines & Lightning"; // because they use the same explosion particles it is almost impossible to differentiate which one killed the player
            public const string TURRET = "Turret";
            public const string EXT_LADDER = "Extension ladder";
            public const string COMPANY_MONSTER = "Jeb (AKA Company Monster)";
            public const string DROWNING = "Drowning";
            public const string ABANDON = "Abandonment";
            public const string FRIENDLY_FIRE = "Friendly fire";
            public const string LIGHTNING_STRIKE = "Lightning";
            public const string QUICKSAND = "Quicksand";
        }

        private static bool CanBeKilled = true;

        [HarmonyPatch(typeof(JesterAI), "killPlayerAnimation")]
        [HarmonyPostfix]
        static void JesterAIPatch(int playerId) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.JESTER);//, StartOfRound.Instance.allPlayerScripts[playerId].playerClientId);
        }

        [HarmonyPatch(typeof(SpringManAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        static void CoilheadAIPatch(Collider other) {
            var playerController = other.GetComponent<PlayerControllerB>();
            if (playerController == null)
                return;
            var id = playerController.playerClientId;

            //just in case someone doesn't sync properly
            if ((playerController.health <= 0 || playerController.isPlayerDead)) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.COILHEAD);
            }
        }

        [HarmonyPatch(typeof(CrawlerAI), "EatPlayerBodyAnimation")]
        [HarmonyPostfix]
        static void ThumperAIPatch(int playerId) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.THUMPER);//, StartOfRound.Instance.allPlayerScripts[playerId].playerClientId);
        }

        [HarmonyPatch(typeof(MouthDogAI), "KillPlayer")]
        [HarmonyPostfix]
        static void DogAIPatch(int playerId) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.DOG);//, StartOfRound.Instance.allPlayerScripts[playerId].playerClientId);
        }

        [HarmonyPatch(typeof(BlobAI), "eatPlayerBody")]
        [HarmonyPostfix]
        static void BlobAIPatch(int playerKilled) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.BLOB);//, StartOfRound.Instance.allPlayerScripts[playerKilled].playerClientId);
        }

        [HarmonyPatch(typeof(PufferAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        static void SporeLizardAIPatch(Collider other) {
            var playerController = other.GetComponent<PlayerControllerB>();
            if (playerController == null)
                return;
            if ((playerController.health <= 0 || playerController.isPlayerDead)) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.SPORE_DOG);//, playerController.playerClientId);
            }
        }

        [HarmonyPatch(typeof(FlowermanAI), "killAnimation")]
        [HarmonyPrefix]
        static void BrackenAIPatch(FlowermanAI __instance) {
            if (__instance.inSpecialAnimationWithPlayer == null)
                return;
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.BRACKEN);//, __instance.inSpecialAnimationWithPlayer.playerClientId);
        }

        [HarmonyPatch(typeof(DressGirlAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        static void GirlAIPatch(DressGirlAI __instance, Collider other) {
            var controller = other.GetComponent<PlayerControllerB>();
            if (controller == null)
                return;

            if (controller == __instance.hauntingPlayer && controller.isPlayerDead) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.GIRL);//, controller.playerClientId);
            }
        }

        [HarmonyPatch(typeof(NutcrackerEnemyAI), "LegKickPlayer")]
        [HarmonyPostfix]
        static void NutcrackerKickAIPatch(int playerId) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.NUTCRACKER);//, StartOfRound.Instance.allPlayerScripts[playerId].playerClientId);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "IHittable.Hit")]
        [HarmonyPostfix]
        static void NutcrackerShotgunAnFriendlyFirePatch(PlayerControllerB __instance, PlayerControllerB playerWhoHit) {
            var player = __instance;
            if (player == null)
                return;

            if (player.isPlayerDead) {
                var cause = playerWhoHit == null ? DeathCauseConstants.NUTCRACKER : DeathCauseConstants.FRIENDLY_FIRE;
                StatisticsTracker.Instance.OnPlayerDeath(cause);//, player.playerClientId);
            }
        }

        [HarmonyPatch(typeof(SandSpiderAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        static void SpiderAIPatch(Collider other) {
            var controller = other.GetComponent<PlayerControllerB>();
            if (controller == null)
                return;

            if ((controller.isPlayerDead || controller.health <= 0)) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.SPIDER);//, controller.playerClientId);
            }
        }

        [HarmonyPatch(typeof(HoarderBugAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        static void LootbugAIPatch(Collider other) {
            var controller = other.GetComponent<PlayerControllerB>();
            if (controller == null)
                return;

            if (controller.health <= 0 || controller.isPlayerDead) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.HOARDING_BUG);//, controller.playerClientId);
            }
        }

        [HarmonyPatch(typeof(BaboonBirdAI), "killPlayerAnimation")]
        [HarmonyPrefix]
        static void BaboonHawkAIPatch(int playerObject) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.BABOON);//, StartOfRound.Instance.allPlayerScripts[playerObject].playerClientId);
        }

        //[HarmonyPatch(typeof(RedLocustBees), "BeesKillPlayer")]
        //[HarmonyPostfix]
        //static void CircuitBeesAIPatch(PlayerControllerB killedPlayer) {
        //    StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.NOT_THE_BEES);//, id);
        //}

        [HarmonyPatch(typeof(MaskedPlayerEnemy), "FinishKillAnimation")]
        [HarmonyPostfix]
        static void MaskedAIPatch(MaskedPlayerEnemy __instance, bool killedPlayer) {
            var player = __instance.inSpecialAnimationWithPlayer;
            if (player == null)
                return;

            if (player.isPlayerDead || killedPlayer) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.MASKED);//, player.playerClientId);
            }
        }

        [HarmonyPatch(typeof(CentipedeAI), "UnclingFromPlayer")]
        [HarmonyPostfix]
        static void SnareFleaAIPatch(PlayerControllerB playerBeingKilled, bool playerDead) {
            if (playerDead) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.SNARE_FLEA);//, playerBeingKilled.playerClientId);
            }
        }

        [HarmonyPatch(typeof(SandWormAI), "EatPlayer")]
        [HarmonyPostfix]
        static void EarthWormAIPatch(PlayerControllerB playerScript) {
            if (CanBeKilled && !playerScript.isPlayerDead) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.WORM);//, playerScript.playerClientId);
                CanBeKilled = false;
                playerScript.StartCoroutine(RestoreAbilityToBeKilledByAMob());
            }
        }

        private static IEnumerator RestoreAbilityToBeKilledByAMob() {
            yield return new WaitForSeconds(1.25f);
            CanBeKilled = true;
        }

        [HarmonyPatch(typeof(DepositItemsDesk), "AnimationGrabPlayer")]
        [HarmonyPostfix]
        static void CompanyMonsterDeath(int playerID) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.COMPANY_MONSTER);//, StartOfRound.Instance.allPlayerScripts[playerID].playerClientId);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
        [HarmonyPostfix]
        static void NonEnemiesDeathPatch(PlayerControllerB __instance, CauseOfDeath causeOfDeath) {
            if (__instance.isPlayerDead) {
                bool isMonster = false;
                string cause = "";
                switch (causeOfDeath) {
                    case CauseOfDeath.Blast: {
                            cause = DeathCauseConstants.MINE;
                            break;
                    }
                    case CauseOfDeath.Drowning: {
                            cause = DeathCauseConstants.DROWNING;
                            break;
                        }

                    case CauseOfDeath.Gunshots: {
                            cause = DeathCauseConstants.TURRET;
                            break;
                        }
                    case CauseOfDeath.Abandoned: {
                            cause = DeathCauseConstants.ABANDON;
                            break;
                        }
                    case CauseOfDeath.Gravity: {
                            cause = DeathCauseConstants.GRAVITY;
                            break;
                        }
                    case CauseOfDeath.Crushing: {
                            cause = DeathCauseConstants.GIANT;
                            isMonster = true;
                            break;
                        }
                    case CauseOfDeath.Electrocution: {
                            cause = DeathCauseConstants.NOT_THE_BEES;
                            isMonster = true;
                            break;
                        }
                    case CauseOfDeath.Suffocation: {
                            cause = DeathCauseConstants.QUICKSAND;
                            break;
                        }
                    //todo: check which kind of death cause is falling extension ladder
                    default:
                        LogDeathCause(causeOfDeath.ToString(), false);
                        break;
                }

                if (!string.IsNullOrEmpty(cause)) {
                    StatisticsTracker.Instance.OnPlayerDeath(cause);
                    LogDeathCause(cause, isMonster);
                }
            }
        }

        private static void LogDeathCause(string deathCause, bool isEnemy) {
            StatTrackerMod.Logger.LogMessage($"You were killed by {deathCause}{(isEnemy ? " enemy" : "")}");
        }
    }
}
