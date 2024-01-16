using HarmonyLib;
using System.Text;
using TerminalApi.Classes;
using static TerminalApi.TerminalApi;
using System.Linq;
using UnityEngine;

namespace LethalCompanyStatTracker.TerminalStuff {

    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalCommandsPatch {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void AddStatsCommands() {
            AddCommand("stats", new CommandInfo() { 
                Category = "Other",
                Description = "Shows your dedication to company and employee statistics.",
                Title = "STATS [category]"
            });

            AddCommand("scrap", new CommandInfo() {
                DisplayTextSupplier = () => {
                    var sb = new StringBuilder();
                    var src = StatisticsTracker.Instance.allCollectedItems;
                    if (src.Count <= 0) {
                        sb.AppendLine("No scrap info. Please get to work.");
                    } else {
                        int totalItems = 0;
                        int totalValue = 0;
                        sb.AppendLine("Collected items:");
                        foreach (var kvp in src) {
                            sb.Append("- ").Append(kvp.Key).Append(" : ").Append(kvp.Value.Count).Append(" units, worth ").Append(kvp.Value.TotalPrice).Append(" in total\n");
                            totalValue += kvp.Value.TotalPrice;
                            totalItems += kvp.Value.Count;
                        }
                        var mostCommon = src.OrderByDescending(kvp => kvp.Value.Count).First();
                        var leastCommon = src.OrderBy(kvp => kvp.Value.Count).First();
                        sb.Append("Most common item: ").Append(mostCommon.Key).Append(", appeared ").Append(mostCommon.Value.Count).Append(" times, worth a total of ").Append(mostCommon.Value.TotalPrice).Append(" credits").Append('\n');
                        sb.Append("Least common item: ").Append(leastCommon.Key).Append(", appeared ").Append(leastCommon.Value.Count).Append(" times, worth a total of ").Append(leastCommon.Value.TotalPrice).Append(" credits").Append('\n');
                        sb.Append("Average item value: ").Append(Mathf.RoundToInt(totalValue / (float)totalItems)).Append(" credits").Append('\n');
                        sb.Append("Total: ").Append(totalItems).Append(" items, worth ").Append(totalValue).Append('\n');
                    }
                    return sb.ToString();
                },
                Category = "Stats",
                Description = "Shows how much scrap you collected and how much it was worth."
            }, "stats");

            AddCommand("deaths", new CommandInfo() {
                DisplayTextSupplier = () => {
                    var src = StatisticsTracker.Instance.causesOfDeath;
                    var sb = new StringBuilder();
                    if (src.Count <= 0) {
                        sb.AppendLine("No specimens/other forces encountered. Please get to work.");
                    } else {
                        sb.AppendLine("All employee deaths:");
                        foreach (var kvp in src) {
                            sb.Append("- ").Append(kvp.Key).Append(", with ").Append(kvp.Value).Append(" kills\n");
                        }
                        var mostKillsBy_Pair = src.OrderByDescending(kvp => kvp.Value).First();
                        var leastKillsBy_Pair = src.OrderBy(kvp => kvp.Value).First();
                        sb.Append("Most lethal: ").Append(mostKillsBy_Pair.Key).Append(", with ").Append(mostKillsBy_Pair.Value).Append(" total kills").Append('\n');
                        sb.Append("Least lethal: ").Append(leastKillsBy_Pair.Key).Append(", with ").Append(leastKillsBy_Pair.Value).Append(" total kills").Append('\n');
                        sb.Append("Total deaths: ").Append(src.Values.Sum()).Append('\n');
                    }
                    return sb.ToString();
                },
                Category = "Stats",
                Description = "Shows potential causes of death of our employees. How tragic."
            }, "stats");

            AddCommand("moons", new CommandInfo() {
                DisplayTextSupplier = () => {
                    var sb = new StringBuilder();
                    var moons = StatisticsTracker.Instance.moonExpeditionsData;
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
                    }
                    return sb.ToString();
                },
                Category = "Stats",
                Description = "Which of the moons were visited and how bad the weather was."
            }, "stats");

            AddCommand("sales", new CommandInfo() {
                DisplayTextSupplier = () => {
                    var sb = new StringBuilder();
                    var src = StatisticsTracker.Instance.allSoldItems;
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
                },
                Category = "Stats",
                Description = "Shows how much scrap was sold and for how much in total."
            }, "stats");

            AddCommand("kills", new CommandInfo() {
                DisplayTextSupplier = () => {
                    var sb = new StringBuilder();
                    var src = StatisticsTracker.Instance.enemiesKilled;

                    if (src.Count <= 0) {
                        sb.AppendLine("No enemy kills data. Please get to work to ensure good safety in the workplace.");
                    } else {
                        sb.AppendLine("Total monsters eradicated:");
                        foreach (var  kvp in src) {
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
                },
                Category = "Stats",
                Description = "Your eradication progress on the moon dwellers."
            }, "stats");

            AddCommand("shopping", new CommandInfo() {
                DisplayTextSupplier = () => {
                    var src = StatisticsTracker.Instance.allBoughtItems;
                    var sb = new StringBuilder();

                    if (src.Count<=0) {
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
                },
                Description = "Shows statistics regarding shopping and expenses.",
                Category = "Stats"
            }, "stats");
        }
    }
}
