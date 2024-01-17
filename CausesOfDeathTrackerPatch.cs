using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

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
            public const string PUTTING_ON_MASK = "Comedy & Tragedy masks";
        }
        //for some problematic enemies who fire the event multiple times (like Earth Leviathan or Coil Heads)
        private static Dictionary<int, (PlayerControllerB, EnemyAI)> EnemyKillDict = new Dictionary<int, (PlayerControllerB, EnemyAI)>();

        [HarmonyPatch(typeof(JesterAI), "killPlayerAnimation")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void JesterAIPatch(int playerId) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.JESTER);//, StartOfRound.Instance.allPlayerScripts[playerId].playerClientId);
        }

        [HarmonyPatch(typeof(SpringManAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void CoilheadAIPatch(SpringManAI __instance, Collider other) {
            var playerController = __instance.MeetsStandardPlayerCollisionConditions(other);
            if (playerController == null)
                return;
            var id = playerController.playerClientId;

            if ((playerController.health <= 0 || playerController.isPlayerDead) && CanBeKilled(playerController, __instance)) {
                HandleKill(DeathCauseConstants.COILHEAD, playerController, __instance);
            }
        }

        [HarmonyPatch(typeof(CrawlerAI), "EatPlayerBodyAnimation")]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        static void ThumperAIPatch(int playerId) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.THUMPER);//, StartOfRound.Instance.allPlayerScripts[playerId].playerClientId);
        }

        [HarmonyPatch(typeof(MouthDogAI), "KillPlayer")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void DogAIPatch(int playerId) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.DOG);//, StartOfRound.Instance.allPlayerScripts[playerId].playerClientId);
        }

        [HarmonyPatch(typeof(BlobAI), "eatPlayerBody")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void BlobAIPatch(int playerKilled) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.BLOB);//, StartOfRound.Instance.allPlayerScripts[playerKilled].playerClientId);
        }

        [HarmonyPatch(typeof(PufferAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
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
        [HarmonyWrapSafe]
        static void BrackenAIPatch(FlowermanAI __instance) {
            if (__instance.inSpecialAnimationWithPlayer == null)
                return;
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.BRACKEN);//, __instance.inSpecialAnimationWithPlayer.playerClientId);
        }

        [HarmonyPatch(typeof(DressGirlAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void GirlAIPatch(DressGirlAI __instance, Collider other) {
            var controller = other.GetComponent<PlayerControllerB>();
            if (controller == null)
                return;

            if (controller == __instance.hauntingPlayer && controller.isPlayerDead && CanBeKilled(controller, __instance)) {
                HandleKill(DeathCauseConstants.GIRL, controller, __instance);
            }
        }

        [HarmonyPatch(typeof(NutcrackerEnemyAI), "LegKickPlayer")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void NutcrackerKickAIPatch(int playerId) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.NUTCRACKER);//, StartOfRound.Instance.allPlayerScripts[playerId].playerClientId);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "IHittable.Hit")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
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
        [HarmonyWrapSafe]
        static void SpiderAIPatch(SandSpiderAI __instance, Collider other) {
            var controller = other.GetComponent<PlayerControllerB>();
            if (controller == null)
                return;

            if ((controller.isPlayerDead || controller.health <= 0) && CanBeKilled(controller, __instance)) {
                HandleKill(DeathCauseConstants.SPIDER, controller, __instance);
            }
        }

        [HarmonyPatch(typeof(HoarderBugAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void LootbugAIPatch(HoarderBugAI __instance, Collider other) {
            var controller = other.GetComponent<PlayerControllerB>();
            if (controller == null)
                return;

            if (controller.health <= 0 || controller.isPlayerDead && CanBeKilled(controller, __instance)) {
                HandleKill(DeathCauseConstants.HOARDING_BUG, controller, __instance);
            }
        }

        [HarmonyPatch(typeof(BaboonBirdAI), "killPlayerAnimation")]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
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
        [HarmonyWrapSafe]
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
        [HarmonyWrapSafe]
        static void SnareFleaAIPatch(PlayerControllerB playerBeingKilled, bool playerDead) {
            if (playerDead) {//TODO: subtract 1 from Quicksand death, 
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.SNARE_FLEA);//, playerBeingKilled.playerClientId);
                //because Snare fleas actually are counted as CauseOfDeath.Suffocation and so also will increase counter for quicksand
                StatisticsTracker.Instance.cumulativeData.causesOfDeath[DeathCauseConstants.QUICKSAND]-=1;
            }
        }

        [HarmonyPatch(typeof(SandWormAI), "EatPlayer")]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        static void EarthWormAIPatch(SandWormAI __instance, PlayerControllerB playerScript) {
            if (!playerScript.isPlayerDead && CanBeKilled(playerScript, __instance)) {
                HandleKill(DeathCauseConstants.WORM, playerScript, __instance);
            }
        }

        [HarmonyPatch(typeof(DepositItemsDesk), "AnimationGrabPlayer")]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        static void CompanyMonsterDeath(int playerID) {
            StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.COMPANY_MONSTER);//, StartOfRound.Instance.allPlayerScripts[playerID].playerClientId);
        }

        private static FieldInfo PlayerKilledByMask_FieldInfo;
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(HauntedMaskItem), "waitForMimicEnemySpawn")]
        static void HauntedMaskDeath(HauntedMaskItem __instance) {
            if (PlayerKilledByMask_FieldInfo == null) {
                PlayerKilledByMask_FieldInfo = __instance.GetType().GetField("previousPlayerHeldBy", BindingFlags.NonPublic);
            }

            var controller = PlayerKilledByMask_FieldInfo.GetValue(__instance) as PlayerControllerB;
            if (controller != null) {
                StatisticsTracker.Instance.OnPlayerDeath(DeathCauseConstants.PUTTING_ON_MASK);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
        [HarmonyWrapSafe]
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

        private static bool CanBeKilled(PlayerControllerB controller, EnemyAI enemy) {
            var entryPresent = EnemyKillDict.TryGetValue(enemy.GetInstanceID(), out var aiControllerPair);
            if (!entryPresent || (controller != aiControllerPair.Item1 && enemy == aiControllerPair.Item2)) {
                return true;
            }
            return false;
        }

        private static void LogDeathCause(string deathCause, bool isEnemy) {
            StatTrackerMod.Logger.LogMessage($"You were killed by {deathCause}{(isEnemy ? " enemy" : "")}");
        }

        private static void HandleKill(string cause, PlayerControllerB controller, EnemyAI enemy) {
            int id = enemy.GetInstanceID();
            EnemyKillDict[id] = (controller, enemy);
            StatisticsTracker.Instance.OnPlayerDeath(cause);
            enemy.StartCoroutine(RemoveMonsterIDFromDict(id));
        }

        private static IEnumerator RemoveMonsterIDFromDict(int id) {
            yield return new WaitForSeconds(1.25f);
            EnemyKillDict.Remove(id);
        }
    }
}
