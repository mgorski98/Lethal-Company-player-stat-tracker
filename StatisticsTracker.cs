using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace LethalCompanyStatTracker {
    public class StatisticsTracker : MonoBehaviour {

        public class ItemData {
            public int count;
            public Item item;
            public int id => item.itemId;
            public int totalPrice;
        }

        private Dictionary<string, ItemData> allCollectedItems = new Dictionary<string, ItemData>();
        private HashSet<GrabbableObject> itemsSnapshot = new HashSet<GrabbableObject>();

        public static StatisticsTracker Instance { get; private set; } = null;

        private void Awake() {
            if (Instance == null)
                Instance = this;
            else Destroy(this);
        }

        void OnDestroy() {
            if (Instance == this)
                Instance = null;
        }

        private void Start() {
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
                        item = item.itemProperties,
                        count = 1,
                        totalPrice = item.scrapValue
                    };
                    allCollectedItems[item.itemProperties.itemName] = data;
                } else {
                    data.count++;
                    data.totalPrice += item.scrapValue;
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

                StatTrackerMod.Logger.LogMessage($"Collected a total of {data.count}x {name}, with total value {data.totalPrice}");
            }
        }

        public GrabbableObject[] GetAllCollectedScrap() {
            return GameObject.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None).Where(go => go.itemProperties.isScrap && (go.isInShipRoom || go.isPocketed)).ToArray();
        }

        private void SaveToPrefs() {
            //todo: save the dictionary to player prefs OR file
        }
    }
}
