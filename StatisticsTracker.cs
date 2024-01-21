using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using TMPro;

namespace LethalCompanyStatTracker {
    public class StatisticsTracker : MonoBehaviour {

        [Serializable]
        public class ItemData {
            public int Count = 0;
            public int TotalPrice = 0;
        }

        [Serializable]
        public class MoonData {
            public Dictionary<LevelWeatherType, int> WeatherExpeditions = new Dictionary<LevelWeatherType, int>();
            [JsonIgnore]
            public int TotalExpeditionsCount => WeatherExpeditions.Sum(kvp => kvp.Value);

            public MoonData() {
                foreach (var weatherType in Enum.GetValues(typeof(LevelWeatherType)).Cast<LevelWeatherType>()) {
                    WeatherExpeditions[weatherType] = 0;
                }
            }
        }

        [Serializable]
        public class PlayerStatisticsData {
            public DefaultDict<string, ItemData> allCollectedItems = new DefaultDict<string, ItemData>(() => new ItemData());
            public DefaultDict<string, MoonData> moonExpeditionsData = new DefaultDict<string, MoonData>(() => new MoonData());
            public DefaultDict<string, int> causesOfDeath = new DefaultDict<string, int>(0);
            public DefaultDict<string, int> enemiesKilled = new DefaultDict<string, int>(0);
            public DefaultDict<string, ItemData> allSoldItems = new DefaultDict<string, ItemData>(() => new ItemData());
            public DefaultDict<string, ItemData> allBoughtItems = new DefaultDict<string, ItemData>(() => new ItemData());
            public DefaultDict<string, int> deathsOnMoons = new DefaultDict<string, int>(0);
            public int currentSuccessfulMissionStreak = 0;
            public int bestMissionStreak = 0;
            public int highestQuotaReached = 0;
            public long totalJumps = 0;
            public long totalSteps = 0;
            public long totalDamage = 0;
            public long totalMoneySpent = 0;
            public int totalTimesQuotaFulfilled = 0;
            public int bodiesInsured = 0;
        }

        [Serializable]
        public class SerializableStats {
            public Dictionary<string, ItemData> allCollectedItems;
            public Dictionary<string, MoonData> moonExpeditionsData;
            public Dictionary<string, int> causesOfDeath;
            public Dictionary<string, int> enemiesKilled;
            public Dictionary<string, ItemData> allSoldItems;
            public Dictionary<string, ItemData> allBoughtItems;
            public Dictionary<string, int> deathsOnMoons;
            public int currentSuccessfulMissionStreak = 0;
            public int bestMissionStreak = 0;
            public int highestQuotaReached = 0;
            public long totalJumps = 0;
            public long totalSteps = 0;
            public long totalDamage = 0;
            public long totalMoneySpent = 0;
            public int totalTimesQuotaFulfilled = 0;
            public int bodiesInsured = 0;

            public SerializableStats(PlayerStatisticsData data) {
                allCollectedItems = new Dictionary<string, ItemData>(data.allCollectedItems);
                moonExpeditionsData = new Dictionary<string, MoonData>(data.moonExpeditionsData);
                causesOfDeath = new Dictionary<string, int>(data.causesOfDeath);
                enemiesKilled = new Dictionary<string, int>(data.enemiesKilled);
                allSoldItems = new Dictionary<string, ItemData>(data.allSoldItems);
                allBoughtItems = new Dictionary<string, ItemData>(data.allBoughtItems);
                deathsOnMoons = new Dictionary<string, int>(data.deathsOnMoons);
                currentSuccessfulMissionStreak = data.currentSuccessfulMissionStreak;
                bestMissionStreak = data.bestMissionStreak;
                highestQuotaReached = data.highestQuotaReached;
                totalSteps = data.totalSteps;
                totalJumps = data.totalJumps;
                totalDamage = data.totalDamage;
                totalMoneySpent = data.totalMoneySpent;
                totalTimesQuotaFulfilled = data.totalTimesQuotaFulfilled;
                bodiesInsured = data.bodiesInsured;
            }

            public SerializableStats() { } //because deserialization fails when this is not present

            public PlayerStatisticsData ToStatsData() {
                var data = new PlayerStatisticsData();
                data.allCollectedItems.CopyFrom(this.allCollectedItems);
                data.moonExpeditionsData.CopyFrom(this.moonExpeditionsData);
                data.allSoldItems.CopyFrom(this.allSoldItems);
                data.allBoughtItems.CopyFrom(this.allBoughtItems);
                data.enemiesKilled.CopyFrom(this.enemiesKilled);
                data.causesOfDeath.CopyFrom(this.causesOfDeath);
                data.deathsOnMoons.CopyFrom(this.deathsOnMoons);
                data.currentSuccessfulMissionStreak = this.currentSuccessfulMissionStreak;
                data.bestMissionStreak = this.bestMissionStreak;
                data.highestQuotaReached = this.highestQuotaReached;
                data.totalDamage = totalDamage;
                data.totalSteps = totalSteps;
                data.totalJumps = totalJumps;
                data.totalMoneySpent = totalMoneySpent;
                data.totalTimesQuotaFulfilled = totalTimesQuotaFulfilled;
                data.bodiesInsured = bodiesInsured;
                return data;
            }
        }


        private HashSet<GrabbableObject> itemsSnapshot = new HashSet<GrabbableObject>();
        public HashSet<GrabbableObject> currentlyCollected = new HashSet<GrabbableObject>();

        public PlayerStatisticsData cumulativeData = new PlayerStatisticsData();
        public PlayerStatisticsData currentSessionData = new PlayerStatisticsData();

        private GrabbableObject[] currentNewItems;

        private string StatsStoreFilePath => Path.Combine(Application.persistentDataPath, "player_stats_data.json");

        public static StatisticsTracker Instance { get; private set; } = null;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            }
            else Destroy(this);
        }

        void OnDestroy() {
            if (Instance == this) {
                Instance = null;
                SaveProgress();
            }
        }

        private void Start() {
            MakeStatsFileCopy();
            LoadProgress();
            SnapshotCollectedItemsOnMoonStart();
            StatTrackerMod.Logger.LogMessage("Initialized the stat tracker module");
        }

        public void SnapshotCollectedItemsOnMoonStart() {
            itemsSnapshot.Clear();
            var shipOrPlayerScrapItems = GetAllCollectedScrap();
            foreach (var item in shipOrPlayerScrapItems) {
                itemsSnapshot.Add(item);
            }
            StatTrackerMod.Logger.LogMessage($"Snapshotted {shipOrPlayerScrapItems.Length} items;");
        }

        public void InitialProcessOnQuit() {
            if (StartOfRound.Instance.allPlayersDead) {
                StatTrackerMod.Logger.LogMessage("No players alive so nothing new was collected :(");
                return;
            }

            //this is called twice just to ensure that beehives (which players usually grab AFTER departure) are counted and shown properly
            var newItems = GetAllCollectedScrap().Where(item => !itemsSnapshot.Contains(item)).ToArray();
            currentNewItems = newItems;
        }

        public void UpdateHighestQuotaReached() {
            var quota = TimeOfDay.Instance.profitQuota;
            StatTrackerMod.Logger.LogMessage("Current profit quota: " + quota);

            if (quota > cumulativeData.highestQuotaReached)
                cumulativeData.highestQuotaReached = quota;
        }

        public void ProcessOnMoonQuit() {
            var allItems = GetAllCollectedScrap();
            var newItems = allItems.Where(item => !itemsSnapshot.Contains(item) && !currentlyCollected.Contains(item)).ToArray();

            foreach (var item in newItems) {
                var name = item.itemProperties.itemName;
                var data = cumulativeData.allCollectedItems[name];
                data.Count++;
                data.TotalPrice += item.scrapValue;
            }
            currentNewItems = newItems;
            var creditsEarned = newItems.Sum(i => i.scrapValue);
            StatTrackerMod.Logger.LogMessage($"Collected {newItems.Length} new items worth a total of {creditsEarned}, updating the prefs...");
            foreach (var item in newItems) {
                StatTrackerMod.Logger.LogMessage($"{item.itemProperties.itemName} : {item.scrapValue}");
            }

            foreach (var itemPair in cumulativeData.allCollectedItems) {
                string name = itemPair.Key;
                ItemData data = itemPair.Value;

                StatTrackerMod.Logger.LogMessage($"Collected a total of {data.Count}x {name}, with total value {data.TotalPrice}");
            }
        }

        public void ShowCollectedItemsDialog() {
            if (currentNewItems.Length > 0) {
                StartCoroutine(ShowCollectedItems(currentNewItems.Sum(i => i.scrapValue), currentNewItems));
            }
        }

        private IEnumerator ShowCollectedItems(int creditsEarned, GrabbableObject[] items) {
            ChangeProfitWindowTitle("Collected:");
            yield return new WaitForSeconds(5.5f);
            StatTrackerMod.Logger.LogMessage("Showing collected items dialog");
            HUDManager.Instance.DisplayCreditsEarning(creditsEarned, items, -1);
            StartCoroutine(ReturnToOldProfitWindowTitle());
        }

        public void StoreShopBoughtItems(int[] boughtItems, Terminal terminal, int totalCost) {
            if (boughtItems.Length <= 0)
                return;
            var costPerItem = totalCost / boughtItems.Length;
            foreach (var itemIndex in boughtItems) {
                var item = terminal.buyableItemsList[itemIndex];
                var data = cumulativeData.allBoughtItems[item.itemName];
                data.Count++;
                data.TotalPrice += costPerItem;
            }
            StatTrackerMod.Logger.LogMessage($"Stored {boughtItems.Length} bought items, worth {totalCost}");
        }

        public void StoreSoldItems(GrabbableObject[] obj) {
            int bodies = 0;
            foreach (var item in obj) {
                if (item is RagdollGrabbableObject) {
                    bodies++;
                    continue;
                }
                var i = item.itemProperties;
                var data = cumulativeData.allSoldItems[i.itemName];
                data.Count++;
                data.TotalPrice += Mathf.RoundToInt(item.scrapValue * StartOfRound.Instance.companyBuyingRate);
            }
            StatTrackerMod.Logger.LogMessage($"Stored {obj.Length - bodies} sold items.");
        }

        private TextMeshProUGUI ProfitWindowTitle;
        public void ChangeProfitWindowTitle(string newTitle) {
            if (ProfitWindowTitle == null) {
                foreach (Transform child in HUDManager.Instance.HUDContainer.transform) {
                    var text = child.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(t => t.text.ToLower() == "paycheck!");
                    if (text != null) {
                        ProfitWindowTitle = text;
                        break;
                    }
                }
            }
            if (ProfitWindowTitle == null)
                return;

            StatTrackerMod.Logger.LogMessage($"TITLE TEXT: {ProfitWindowTitle.text}");
            ProfitWindowTitle.text = newTitle;
        }

        public IEnumerator ReturnToOldProfitWindowTitle() {
            yield return new WaitForSeconds(10f);
            ChangeProfitWindowTitle("Paycheck!");
        }

        public GrabbableObject[] GetAllCollectedScrap() {
            return GameObject.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None).Where(go => go.itemProperties.isScrap && (go.isInShipRoom || go.isPocketed || go.isHeld) && !(go is RagdollGrabbableObject)).ToArray();
        }

        #region Progress persistence
        private void SaveProgress() {
            var serializableCumulativeData = new SerializableStats(cumulativeData);
            var json = JsonConvert.SerializeObject(serializableCumulativeData, Formatting.Indented);
            StatTrackerMod.Logger.LogMessage(json);
            File.WriteAllText(StatsStoreFilePath, json);
            StatTrackerMod.Logger.LogMessage($"Saved global stats progress!");
        }

        private void LoadProgress() {
            var path = StatsStoreFilePath;

            if (!File.Exists(path)) {
                StatTrackerMod.Logger.LogMessage($"Stats file does not exist, starting fresh...");
                return;
            }

            var json = File.ReadAllText(path);
            var serializableCumulativeData = JsonConvert.DeserializeObject<SerializableStats>(json);
            StatTrackerMod.Logger.LogMessage(json);
            cumulativeData = serializableCumulativeData.ToStatsData();
            StatTrackerMod.Logger.LogMessage("Loaded stats file.");
        }

        private void MakeStatsFileCopy() {
            var path = StatsStoreFilePath;

            if (!File.Exists(path)) {
                StatTrackerMod.Logger.LogMessage("Stats file does not exist, cannot copy");
                return;
            }

            File.Copy(path, Path.Combine(Application.persistentDataPath, "player_stats_data_copy.json"), true);
            StatTrackerMod.Logger.LogMessage("Successfully made a backup of stats file");
        }

        #endregion

        #region Callbacks
        public void UpdatePlanetExpeditionData(SelectableLevel level) {
            var planetName = level.PlanetName;
            var weather = level.currentWeather;

            var data = cumulativeData.moonExpeditionsData[planetName];
            data.WeatherExpeditions[weather] += 1;
        }

        public void ShowStreakLostInfo(bool playersWereDead) {
            StatTrackerMod.Logger.LogMessage($"all players dead? {playersWereDead}");
            if (playersWereDead) {
                if (cumulativeData.currentSuccessfulMissionStreak <= 0) {
                    StatTrackerMod.Logger.LogMessage("No successful missions yet, skipping...");
                    return;
                }
                string header;
                string body;
                if (cumulativeData.currentSuccessfulMissionStreak > cumulativeData.bestMissionStreak) {
                    header = "New streak record!";
                    body = $"You made a new mission streak record! A total of {cumulativeData.currentSuccessfulMissionStreak} successful missions in a row! Congrats!";
                    cumulativeData.bestMissionStreak = cumulativeData.currentSuccessfulMissionStreak;
                } else {
                    header = "Streak lost!";
                    body = $"You reached {cumulativeData.currentSuccessfulMissionStreak} successful missions in a row!";
                }
                HUDManager.Instance.DisplayTip(header, body);
                cumulativeData.currentSuccessfulMissionStreak = 0;
            } else {
                if (!RoundManager.Instance.currentLevel.IsCompanyBuilding()) {
                    cumulativeData.currentSuccessfulMissionStreak++;
                    StatTrackerMod.Logger.LogMessage($"Current streak: {cumulativeData.currentSuccessfulMissionStreak}");
                }
            }
        }

        public void OnPlayerDeath(string causeOfDeath_Name) {
            if (string.IsNullOrWhiteSpace(causeOfDeath_Name))
                return;
            var old = cumulativeData.causesOfDeath[causeOfDeath_Name];
            cumulativeData.causesOfDeath[causeOfDeath_Name] += 1;

            //todo: store the information on which moon the death occurred
            var moon = RoundManager.Instance.currentLevel;
            cumulativeData.deathsOnMoons[moon.PlanetName]++;
            StatTrackerMod.Logger.LogMessage($"death caused by: {causeOfDeath_Name}. Current: {cumulativeData.causesOfDeath[causeOfDeath_Name]}, old: {old}");
            StatTrackerMod.Logger.LogMessage($"death occurred on {moon.PlanetName} moon");
        }

        public void OnEnemyKilled(string enemyKilledName) {
            var old = cumulativeData.enemiesKilled[enemyKilledName];
            cumulativeData.enemiesKilled[enemyKilledName] += 1;
            StatTrackerMod.Logger.LogMessage($"Killed a total of {cumulativeData.enemiesKilled[enemyKilledName]} enemies (previously {old})");
        }

        #endregion
    }
}
