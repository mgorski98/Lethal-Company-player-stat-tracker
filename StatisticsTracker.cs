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
            public int Count;
            public string ItemName;
            public int TotalPrice;
        }

        [Serializable]
        public class MoonData {
            public string MoonName;
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
            public Dictionary<string, ItemData> allCollectedItems;
            public Dictionary<string, MoonData> moonExpeditionsData;
            public Dictionary<string, int> causesOfDeath;
            public Dictionary<string, int> enemiesKilled;
            public Dictionary<string, ItemData> allSoldItems;
        }


        private HashSet<GrabbableObject> itemsSnapshot = new HashSet<GrabbableObject>();
        private Dictionary<string, ItemData> allCollectedItems = new Dictionary<string, ItemData>();
        private Dictionary<string, MoonData> moonExpeditionsData = new Dictionary<string, MoonData>();
        private Dictionary<string, int> causesOfDeath = new Dictionary<string, int>();
        private Dictionary<string, int> enemiesKilled = new Dictionary<string, int>();
        private Dictionary<string, ItemData> allSoldItems = new Dictionary<string, ItemData>();

        private GrabbableObject[] currentNewItems;

        private string StatsStoreFilePath => Path.Combine(Application.persistentDataPath, "player_stats_data.json");

        public static StatisticsTracker Instance { get; private set; } = null;

        private void Awake() {
            if (Instance == null)
                Instance = this;
            else Destroy(this);
        }

        void OnDestroy() {
            if (Instance == this) {
                Instance = null;
                SaveProgress();
            }
        }

        private void Start() {
            LoadProgress();
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

        public void ProcessOnMoonQuit() {
            if (StartOfRound.Instance.allPlayersDead) {
                StatTrackerMod.Logger.LogMessage("No players alive so nothing new was collected :(");
                return;
            }
            var allItems = GetAllCollectedScrap();
            var newItems = allItems.Where(item => !itemsSnapshot.Contains(item)).ToArray();

            foreach (var item in newItems) {
                if (!allCollectedItems.TryGetValue(item.itemProperties.itemName, out var data)) {
                    data = new ItemData {
                        ItemName = item.itemProperties.itemName,
                        Count = 1,
                        TotalPrice = item.scrapValue
                    };
                    allCollectedItems[item.itemProperties.itemName] = data;
                } else {
                    data.Count++;
                    data.TotalPrice += item.scrapValue;
                }
            }
            currentNewItems = newItems;
            var creditsEarned = newItems.Sum(i => i.scrapValue);
            StatTrackerMod.Logger.LogMessage($"Collected {newItems.Length} new items worth a total of {creditsEarned}, updating the prefs...");
            foreach (var item in newItems) {
                StatTrackerMod.Logger.LogMessage($"{item.itemProperties.itemName} : {item.scrapValue}");
            }

            foreach (var itemPair in allCollectedItems) {
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

        public void StoreSoldItems(GrabbableObject[] obj) {
            foreach (var item in obj) {
                var i = item.itemProperties;
                if (allSoldItems.TryGetValue(i.itemName, out var data)) {
                    var newData = new ItemData() {
                        ItemName = i.itemName,
                        Count = 1,
                        TotalPrice = item.scrapValue
                    };
                    allSoldItems[i.itemName] = newData;
                } else {
                    data.Count++;
                    data.TotalPrice += item.scrapValue;
                }
            }
            StatTrackerMod.Logger.LogMessage($"Stored {obj.Length} sold items.");
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
            return GameObject.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None).Where(go => go.itemProperties.isScrap && (go.isInShipRoom || go.isPocketed)).ToArray();
        }

        #region Progress persistence
        private void SaveProgress() {
            var data = new PlayerStatisticsData {
                enemiesKilled = this.enemiesKilled,
                allCollectedItems = this.allCollectedItems,
                causesOfDeath = this.causesOfDeath, 
                moonExpeditionsData = this.moonExpeditionsData,
                allSoldItems = this.allSoldItems
            };

            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(StatsStoreFilePath, json);
            StatTrackerMod.Logger.LogMessage($"Saved stats progress!");
        }

        private void LoadProgress() {
            var path = StatsStoreFilePath;

            if (!File.Exists(path)) {
                StatTrackerMod.Logger.LogMessage($"Stats file does not exist, starting fresh...");
                return;
            }

            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<PlayerStatisticsData>(json);
            allCollectedItems = data.allCollectedItems;
            causesOfDeath = data.causesOfDeath;
            enemiesKilled = data.enemiesKilled;
            moonExpeditionsData = data.moonExpeditionsData;
            allSoldItems = data.allSoldItems;
            StatTrackerMod.Logger.LogMessage("Loaded stats file.");
        }
        #endregion

        public void UpdatePlanetExpeditionData(SelectableLevel level) {
            var planetName = level.PlanetName;
            var weather = level.currentWeather;

            if (!moonExpeditionsData.TryGetValue(planetName, out var data)) {
                var newData = new MoonData() {
                    MoonName = planetName
                };

                newData.WeatherExpeditions[weather] = 1;
                moonExpeditionsData[planetName] = newData;
            } else {
                if (!data.WeatherExpeditions.ContainsKey(weather)) {
                    data.WeatherExpeditions[weather] = 1;
                } else {
                    data.WeatherExpeditions[weather] = data.WeatherExpeditions[weather] + 1;
                }
            }
        }

        public void OnPlayerDeath(string causeOfDeath_Name, ulong playerId = ulong.MaxValue) {
            var localPlayerId = GameNetworkManager.Instance.localPlayerController.playerClientId;
            bool validPlayer = localPlayerId == playerId;
            if (!validPlayer)
                return;

            if (!causesOfDeath.TryGetValue(causeOfDeath_Name, out int count)) {
                causesOfDeath[causeOfDeath_Name] = 1;
                StatTrackerMod.Logger.LogMessage($"first death by: {causeOfDeath_Name}");
            } else {
                causesOfDeath[causeOfDeath_Name] = count + 1;
                StatTrackerMod.Logger.LogMessage($"death caused by: {causeOfDeath_Name}. Current: {count+1}, old: {count}");
            }

            foreach (var kvp in causesOfDeath) {
                StatTrackerMod.Logger.LogMessage($"Deaths by {kvp.Key}: {kvp.Value}");
            }
        }

        public void OnEnemyKilled(string enemyKilledName, ulong playerId = ulong.MaxValue) {
            if (playerId != GameNetworkManager.Instance.localPlayerController.playerClientId)
                return;

            if (!enemiesKilled.TryGetValue(enemyKilledName, out int count)) {
                enemiesKilled[enemyKilledName] = 1;
            } else {
                enemiesKilled[enemyKilledName] = count+1;
            }

            StatTrackerMod.Logger.LogMessage($"Killed a total of {count + 1} enemies (previously {count})");
        }
    }
}
