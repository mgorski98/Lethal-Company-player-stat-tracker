using HarmonyLib;
using System.Text;
using TerminalApi.Classes;
using static TerminalApi.TerminalApi;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace LethalCompanyStatTracker.TerminalStuff {

    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalCommandsPatch {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void AddStatsCommands() {
            //todo: add commands for current session stats display
            AddCommand("stats", new CommandInfo() { 
                Category = "Other",
                Description = "Shows your dedication to company and employee statistics.",
                Title = "STATS [category]"
            });

            AddCommand("curstats", new CommandInfo() { 
                Category = "Other",
                Title = "CURSTATS [category]",
                Description = "Shows your current performance statistics."
            });

            AddCommand("scrap", new CommandInfo() {
                DisplayTextSupplier = () => FormatCollectedScrap(StatisticsTracker.Instance.cumulativeData.allCollectedItems),
                Category = "Stats",
                Description = "Shows how much scrap you collected and how much it was worth."
            }, "stats");

            AddCommand("deaths", new CommandInfo() {
                DisplayTextSupplier = () => FormatCausesOfDeath(StatisticsTracker.Instance.cumulativeData.causesOfDeath),
                Category = "Stats",
                Description = "Shows potential causes of death of our employees. How tragic."
            }, "stats");

            AddCommand("moons", new CommandInfo() {
                DisplayTextSupplier = () => FormatMoonExpeditions(StatisticsTracker.Instance.cumulativeData.moonExpeditionsData, StatisticsTracker.Instance.cumulativeData.bestMissionStreak),
                Category = "Stats",
                Description = "Which of the moons were visited and how bad the weather was."
            }, "stats");

            AddCommand("sales", new CommandInfo() {
                DisplayTextSupplier = () => FormatSoldScrap(StatisticsTracker.Instance.cumulativeData.allSoldItems),
                Category = "Stats",
                Description = "Shows how much scrap was sold and for how much in total."
            }, "stats");

            AddCommand("kills", new CommandInfo() {
                DisplayTextSupplier = () => FormatKilledEnemies(StatisticsTracker.Instance.cumulativeData.enemiesKilled),
                Category = "Stats",
                Description = "Your eradication progress on the moon dwellers."
            }, "stats");

            AddCommand("shopping", new CommandInfo() {
                DisplayTextSupplier = () => FormatBoughtItems(StatisticsTracker.Instance.cumulativeData.allBoughtItems),
                Description = "Shows statistics regarding shopping and expenses.",
                Category = "Stats"
            }, "stats");
        }

        private static string FormatCollectedScrap(Dictionary<string, StatisticsTracker.ItemData> scrapData) {
            var sb = new StringBuilder();
            var src = scrapData;
            if (src.Count <= 0) {
                sb.AppendLine("No scrap info. Please get to work.");
            } else {
                int totalItems = 0;
                int totalValue = 0;
                sb.AppendLine("Collected items:");
                foreach (var kvp in src) {
                    sb.AppendLine($"- {kvp.Key} : {kvp.Value.Count} units, worth {kvp.Value.TotalPrice}");
                    totalValue += kvp.Value.TotalPrice;
                    totalItems += kvp.Value.Count;
                }
                var mostCommon = src.OrderByDescending(kvp => kvp.Value.Count).First();
                var leastCommon = src.OrderBy(kvp => kvp.Value.Count).First();
                sb.AppendLine($"Most common item: {mostCommon.Key}, appeared {mostCommon.Value.Count} times, worth a total of {mostCommon.Value.TotalPrice} credits");
                sb.AppendLine($"Least common item: {leastCommon.Key}, appeared {leastCommon.Value.Count} times, worth a total of {leastCommon.Value.TotalPrice} credits");
                sb.AppendLine($"Average item value: {Mathf.RoundToInt(totalValue / (float)totalItems)} credits");
                sb.AppendLine($"Total: {totalItems}, worth {totalValue} credits");
            }
            return sb.ToString();
        }

        private static string FormatSoldScrap(Dictionary<string, StatisticsTracker.ItemData> scrapData) {
            var sb = new StringBuilder();
            var src = scrapData;
            if (src.Count <= 0) {
                sb.AppendLine("No scrap sold yet. Please get to work to ensure The Company's well being.");
            } else {
                sb.AppendLine("Scrap sold:");
                foreach (var kvp in src) {
                    sb.Append("- ").Append(kvp.Key).Append(" : ").Append(kvp.Value.Count).Append(" units, worth ").Append(kvp.Value.TotalPrice).Append(" in total\n");
                }
                var totalIncome = src.Values.Sum(i => i.TotalPrice);
                var averageIncome = Mathf.RoundToInt(totalIncome / (float)src.Values.Sum(i => i.Count));
                var mostSoldItem = src.OrderByDescending(kvp => kvp.Value.Count).First();
                var leastSoldItem = src.OrderBy(kvp => kvp.Value.Count).First();
                sb.Append("Most sold item: ").Append(mostSoldItem.Key).Append(", sold a total of ").Append(mostSoldItem.Value.Count).Append(" times for ").Append(mostSoldItem.Value.TotalPrice).Append(" credits\n");
                sb.Append("Least sold item: ").Append(leastSoldItem.Key).Append(", sold a total of ").Append(leastSoldItem.Value.Count).Append(" times for ").Append(leastSoldItem.Value.TotalPrice).Append(" credits\n");
                sb.Append("Average income: ").Append(averageIncome).Append(" credits\n");
                sb.Append("Total income: ").Append(totalIncome).Append(" credits\n");
                sb.AppendLine("The Company is proud of you.");
            }
            return sb.ToString();
        }

        private static string FormatMoonExpeditions(Dictionary<string, StatisticsTracker.MoonData> moonsData, int bestStreak) {
            var sb = new StringBuilder();
            var moons = moonsData;
            if (moons.Count <= 0)
                sb.AppendLine("No expeditions yet, please get to work.");
            else {
                sb.AppendLine("Moons visited: ");
                foreach (var moonData in moons) {
                    sb.Append("- ").Append(moonData.Key).Append(", visited ").Append(moonData.Value.TotalExpeditionsCount).Append(" times\n");

                    foreach (var weatherData in moonData.Value.WeatherExpeditions) {
                        if (weatherData.Value <= 0)
                            continue;

                        var weather = weatherData.Key;
                        sb.Append("\t- ").Append(weather == LevelWeatherType.None ? "Normal weather" : weather.ToString()).Append(", visited ").Append(weatherData.Value).Append(" times\n");
                    }
                    sb.AppendLine();
                }
                sb.AppendLine($"Best moon streak: {bestStreak} missions in a row");
            }
            return sb.ToString();
        }

        private static string FormatCausesOfDeath(Dictionary<string, int> deathsData) {
            var src = deathsData;
            var sb = new StringBuilder();
            if (src.Count <= 0) {
                sb.AppendLine("No specimens/other forces encountered. Please get to work.");
            } else {
                sb.AppendLine("All employee deaths:");
                foreach (var kvp in src) {
                    sb.AppendLine($"- {kvp.Key}, with {kvp.Value} kills");
                }
                var mostKillsBy_Pair = src.OrderByDescending(kvp => kvp.Value).First();
                var leastKillsBy_Pair = src.OrderBy(kvp => kvp.Value).First();
                sb.AppendLine($"Most lethal: {mostKillsBy_Pair.Key}, with {mostKillsBy_Pair.Value} total kills");
                sb.AppendLine($"Least lethal: {leastKillsBy_Pair.Key}, with {leastKillsBy_Pair.Value} total kills");
                sb.AppendLine($"Total deaths: {src.Values.Sum()}");
            }
            return sb.ToString();
        }

        private static string FormatKilledEnemies(Dictionary<string, int> killsData) {
            var sb = new StringBuilder();
            var src = killsData;

            if (src.Count <= 0) {
                sb.AppendLine("No enemy kills data. Please get to work to ensure good safety in the workplace.");
            } else {
                sb.AppendLine("Total monsters eradicated:");
                foreach (var kvp in src) {
                    sb.Append("- ").Append(kvp.Key).Append(" : ").Append(kvp.Value).Append(" total kills\n");
                }

                var mostKilled_pair = src.OrderByDescending(kvp => kvp.Value).First();
                var leastKilled_pair = src.OrderBy(kvp => kvp.Value).First();
                var totalKills = src.Values.Sum();
                sb.Append("Least intellectually advanced: ").Append(mostKilled_pair.Key).Append(", killed a total of ").Append(mostKilled_pair.Value).Append(" times\n");
                sb.Append("Most elusive/powerful: ").Append(leastKilled_pair.Key).Append(", killed a total of ").Append(leastKilled_pair.Value).Append(" times\n");
                sb.Append("Total kills: ").Append(totalKills).Append(" nuisances eliminated\n");
                sb.AppendLine("The Company values your eradication efforts.");
            }

            return sb.ToString();
        }

        private static string FormatBoughtItems(Dictionary<string, StatisticsTracker.ItemData> itemsData) {
            var src = itemsData;
            var sb = new StringBuilder();

            if (src.Count <= 0) {
                sb.AppendLine("No items bought yet. The company store misses you.");
            } else {
                sb.AppendLine("Bought items:");
                foreach (var kvp in src) {
                    sb.Append("- ").Append(kvp.Value.Count).Append("x ").Append(kvp.Key).Append(", costing a total of ").
                        Append(kvp.Value.TotalPrice).Append(" credits").Append('\n');
                }

                var mostBought_pair = src.OrderByDescending(kvp => kvp.Value.Count).First();
                var leastBought_pair = src.OrderBy(kvp => kvp.Value.Count).First();
                sb.Append("Most bought item: ").Append(mostBought_pair.Key).Append(", bought ").Append(mostBought_pair.Value.Count).Append(" times\n");
                sb.Append("Least bought item: ").Append(leastBought_pair.Key).Append(", bought ").Append(leastBought_pair.Value.Count).Append(" times\n");
                sb.Append("Total bought: ").Append(src.Values.Sum(d => d.Count)).Append(" items, worth a total of ").Append(src.Values.Sum(d => d.TotalPrice)).Append(" credits\n");
            }

            return sb.ToString();
        }
    }
}
