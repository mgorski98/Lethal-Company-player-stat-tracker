using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using System.IO;

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
            public int TotalExpeditionsCount => WeatherExpeditions.Sum(kvp => kvp.Value);

            public MoonData() {
                foreach (var weatherType in Enum.GetValues(typeof(LevelWeatherType)).Cast<LevelWeatherType>()) {
                    WeatherExpeditions[weatherType] = 0;
                }
            }
        }

        private HashSet<GrabbableObject> itemsSnapshot = new HashSet<GrabbableObject>();
        private Dictionary<string, ItemData> allCollectedItems = new Dictionary<string, ItemData>();
        private Dictionary<string, MoonData> moonExpeditionsData = new Dictionary<string, MoonData>();
        private Dictionary<string, int> causesOfDeath = new Dictionary<string, int>();
        private Dictionary<string, int> enemiesKilled = new Dictionary<string, int>();

        private string StatsStoreFilePath => Path.Combine(Application.persistentDataPath, "player_stats.json");

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

            //todo: show popup
            StatTrackerMod.Logger.LogMessage($"Collected {newItems.Length} new items worth a total of {newItems.Sum(i => i.scrapValue)}, updating the prefs...");
            foreach (var item in newItems) {
                StatTrackerMod.Logger.LogMessage($"{item.itemProperties.itemName} : {item.scrapValue}");
            }

            foreach (var itemPair in allCollectedItems) {
                string name = itemPair.Key;
                ItemData data = itemPair.Value;

                StatTrackerMod.Logger.LogMessage($"Collected a total of {data.Count}x {name}, with total value {data.TotalPrice}");
            }
        }

        public GrabbableObject[] GetAllCollectedScrap() {
            return GameObject.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None).Where(go => go.itemProperties.isScrap && (go.isInShipRoom || go.isPocketed)).ToArray();
        }

        private void SaveProgress() {
            //todo: save the dictionary to player prefs OR file
        }

        private void LoadProgress() {
            var path = StatsStoreFilePath;

            if (!File.Exists(path)) {
                return;
            }


        }

        public void UpdatePlanetExpeditionData(SelectableLevel level) {
            var planetName = level.PlanetName;
            var weather = level.currentWeather;

            if (!moonExpeditionsData.TryGetValue(planetName, out var data)) {
                var newData = new MoonData() {
                    MoonName = planetName
                };

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
            //prawdopodobnie trzeba będzie przekazać też tutaj ID gracza i sprawdzić czy jest równe ID lokalnego gracza i dopiero wtedy dodać
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
    }
}
