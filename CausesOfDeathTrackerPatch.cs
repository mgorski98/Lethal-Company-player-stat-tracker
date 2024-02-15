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
            public const string GRAVITY = "Gravity";
            public const string WORM = "Earth leviathan";
            public const string GIRL = "Ghost girl";
            public const string BABOON = "Baboon hawk";
            public const string MASKED = "Masked";
            public const string SPORE_DOG = "Spore lizard";
            public const string BRACKEN = "Bracken";
            public const string NUTCRACKER = "Nutcracker";
            public const string SPIDER = "Bunker spider";
            public const string NOT_THE_BEES = "Circuit bees";
            public const string BLAST = "Explosions"; // because they use the same explosion particles it is almost impossible to differentiate which one killed the player
            public const string GUNSHOTS = "Gunshots";
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
        private const int COIL_HEAD_KILL_ANIM_ID = 2;

        private static StatisticsTracker Tracker => StatisticsTracker.Instance;

        [HarmonyPatch(typeof(JesterAI), "KillPlayerClientRpc")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void JesterAIPatch(int playerId) {
            Tracker.SetPlayerKilled(playerId, DeathCauseConstants.JESTER);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        static void CoilheadAIPatch(PlayerControllerB __instance, int deathAnimation) {
            if (deathAnimation == COIL_HEAD_KILL_ANIM_ID) {
                Tracker.SetPlayerKilled((int)__instance.playerClientId, DeathCauseConstants.COILHEAD);
            }
        }

        [HarmonyPatch(typeof(CrawlerAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void ThumperAIPatch(CrawlerAI __instance, Collider other) {
            var controller = __instance.MeetsStandardPlayerCollisionConditions(other);
            if (controller == null) {
                return;
            }
            if (controller.health <= 0 || controller.isPlayerDead) {
                Tracker.SetPlayerKilled((int)controller.playerClientId, DeathCauseConstants.THUMPER);
            }
        }

        [HarmonyPatch(typeof(MouthDogAI), "KillPlayer")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void DogAIPatch(int playerId) {
            Tracker.SetPlayerKilled(playerId, DeathCauseConstants.DOG);
        }

        [HarmonyPatch(typeof(BlobAI), "eatPlayerBody")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void BlobAIPatch(int playerKilled) {
            Tracker.SetPlayerKilled(playerKilled, DeathCauseConstants.BLOB);
        }

        [HarmonyPatch(typeof(PufferAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void SporeLizardAIPatch(Collider other) {
            var playerController = other.GetComponent<PlayerControllerB>();
            if (playerController == null)
                return;
            if (playerController.health <= 0 || playerController.isPlayerDead) {
                Tracker.SetPlayerKilled((int)playerController.playerClientId, DeathCauseConstants.SPORE_DOG);
            }
        }

        [HarmonyPatch(typeof(FlowermanAI), "killAnimation")]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        static void BrackenAIPatch(FlowermanAI __instance) {
            if (__instance.inSpecialAnimationWithPlayer == null)
                return;
            Tracker.SetPlayerKilled((int)__instance.inSpecialAnimationWithPlayer.playerClientId, DeathCauseConstants.BRACKEN);
        }

        [HarmonyPatch(typeof(DressGirlAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void GirlAIPatch(DressGirlAI __instance, Collider other) {
            var controller = other.GetComponent<PlayerControllerB>();
            if (controller == null)
                return;

            if (controller == __instance.hauntingPlayer && (controller.isPlayerDead || controller.health <= 0)) {
                Tracker.SetPlayerKilled((int)controller.playerClientId, DeathCauseConstants.GIRL);
            }
        }

        [HarmonyPatch(typeof(NutcrackerEnemyAI), "LegKickPlayer")]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        static void NutcrackerKickAIPatch(NutcrackerEnemyAI __instance, int playerId) {
            var controller = StartOfRound.Instance.allPlayerScripts[playerId];
            if (controller == null)
                return;

            Tracker.SetPlayerKilled(playerId, DeathCauseConstants.NUTCRACKER);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayerFromOtherClientClientRpc")]
        static void FriendlyFirePatch(PlayerControllerB __instance, int newHealthAmount) {
            if (newHealthAmount <= 0) {
                StatTrackerMod.Logger.LogMessage("Player killed with friendly fire");

                Tracker.SetPlayerKilled((int)__instance.playerClientId, DeathCauseConstants.FRIENDLY_FIRE);
            }
        }

        [HarmonyPatch(typeof(SandSpiderAI), "OnCollideWithPlayer")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void SpiderAIPatch(SandSpiderAI __instance, Collider other) {
            var controller = other.GetComponent<PlayerControllerB>();
            if (controller == null)
                return;

            if (controller.isPlayerDead || controller.health <= 0) {
                Tracker.SetPlayerKilled((int)controller.playerClientId, DeathCauseConstants.SPIDER);
            }
        }

        [HarmonyPatch(typeof(HoarderBugAI), "OnCollideWithPlayer")]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        static void LootbugAIPatch(HoarderBugAI __instance, Collider other) {
            var controller = __instance.MeetsStandardPlayerCollisionConditions(other);
            if (controller == null)
                return;

            if (controller.isPlayerDead || controller.health - 30 <= 0)
                Tracker.SetPlayerKilled((int)controller.playerClientId, DeathCauseConstants.HOARDING_BUG);
        }

        [HarmonyPatch(typeof(BaboonBirdAI), "killPlayerAnimation")]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        static void BaboonHawkAIPatch(int playerObject) {
            Tracker.SetPlayerKilled(playerObject, DeathCauseConstants.BABOON);
        }

        [HarmonyPatch(typeof(MaskedPlayerEnemy), "FinishKillAnimation")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void MaskedAIPatch(MaskedPlayerEnemy __instance, bool killedPlayer) {
            var player = __instance.inSpecialAnimationWithPlayer;
            if (player == null)
                return;

            if (player.isPlayerDead || killedPlayer) {
                Tracker.SetPlayerKilled((int)player.playerClientId, DeathCauseConstants.MASKED);
            }
        }

        [HarmonyPatch(typeof(CentipedeAI), "UnclingFromPlayer")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void SnareFleaAIPatch(PlayerControllerB playerBeingKilled, bool playerDead) {
            if (playerDead) {
                Tracker.SetPlayerKilled((int)playerBeingKilled.playerClientId, DeathCauseConstants.SNARE_FLEA);
            }
        }

        [HarmonyPatch(typeof(SandWormAI), "EatPlayer")]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        static void EarthWormAIPatch(SandWormAI __instance, PlayerControllerB playerScript) {
            Tracker.SetPlayerKilled((int)playerScript.playerClientId, DeathCauseConstants.WORM);
        }

        [HarmonyPatch(typeof(DepositItemsDesk), "AnimationGrabPlayer")]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        static void CompanyMonsterDeath(int playerID) {
            Tracker.SetPlayerKilled(playerID, DeathCauseConstants.COMPANY_MONSTER);
        }

        private static FieldInfo PlayerKilledByMask_FieldInfo;
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(HauntedMaskItem), "CreateMimicClientRpc")]
        static void HauntedMaskDeath(HauntedMaskItem __instance) {
            if (PlayerKilledByMask_FieldInfo == null) {
                PlayerKilledByMask_FieldInfo = __instance.GetType().GetField("previousPlayerHeldBy", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            var controller = PlayerKilledByMask_FieldInfo.GetValue(__instance) as PlayerControllerB;
            Tracker.SetPlayerKilled((int)controller.playerClientId, DeathCauseConstants.PUTTING_ON_MASK);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayerClientRpc")]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        static void NonEnemiesDeathPatch(PlayerControllerB __instance, int causeOfDeath) {
            if (__instance.isPlayerDead) {
                var causeOfDeathEnum = (CauseOfDeath)causeOfDeath;
                bool isMonster = false;
                string cause = "";
                switch (causeOfDeathEnum) {
                    case CauseOfDeath.Blast: {
                            cause = DeathCauseConstants.BLAST;
                            break;
                        }
                    case CauseOfDeath.Drowning: {
                            cause = DeathCauseConstants.DROWNING;
                            break;
                        }

                    case CauseOfDeath.Gunshots: {
                            cause = DeathCauseConstants.GUNSHOTS;
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
                    Tracker.SetPlayerKilled((int)__instance.playerClientId, cause);
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
