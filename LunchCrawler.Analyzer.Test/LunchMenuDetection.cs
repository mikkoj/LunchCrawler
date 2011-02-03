using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

using HtmlAgilityPack;

using Lunch.Data;

using LunchCrawler.Common.Enums;


namespace LunchCrawler.Analyzer.Test
{
    public static class LunchMenuDetection
    {
        private static readonly IList<string> WeekDays = File.ReadAllLines("WeekDays.txt").ToList();
        private const string WeekPattern = @"(viikko|vko|week) (\d+)";
        private const string MoneyPattern = @"\d+([.,]\d+)?\s*?[€e]|[^.](\d+[.,]\d\d)";

        private static readonly IList<string> SkipPatterns = File.ReadAllLines("SkipPatterns.txt").ToList();
        private static readonly IList<FoodKeyword> FoodKeywords = new LunchDA().GetAllFoodKeywords();

        
        public static LunchFeature DetectFeature(HtmlNode node)
        {
            var feature = new LunchFeature
            {
                Type = LunchFeatureType.Unknown,
                FoodMatchType = FoodMatchType.NoMatch
            };

            if (node.InnerText == null)
            {
                return feature;
            }

            var nodeText = HttpUtility.HtmlDecode(node.InnerText.ToLower().Trim());
            if (nodeText == null)
            {
                return feature;
            }


            // check node for patterns to be skipped
            if (string.IsNullOrEmpty(nodeText) ||
                SkipPatterns.Any(skipPattern => Regex.IsMatch(nodeText, skipPattern)))
            {
                feature.Type = LunchFeatureType.Unknown;
            }

            // try to detect weekdays and week
            if (WeekDays.Any(d => d.Equals(nodeText)))
            {
                feature.Type = LunchFeatureType.Weekday;
            }

            if (Regex.IsMatch(nodeText, WeekPattern))
            {
                feature.Type = LunchFeatureType.Week;
            }

            // -----------------------------------------------------------------------------------------------------
            // try to detect food and money

            feature.FoodMatchType = DetectFood(nodeText);
            var foodDetected = feature.FoodMatchType != FoodMatchType.NoMatch;

            var moneyDetected = Regex.IsMatch(nodeText, MoneyPattern);
            
            if (foodDetected && moneyDetected)
            {
                feature.Type = LunchFeatureType.FoodAndMoney;
            }

            if (foodDetected)
            {
                feature.Type = LunchFeatureType.Food;
            }

            if (moneyDetected)
            {
                feature.Type = LunchFeatureType.Money;
            }


            // only complete the feature if it was recognized
            if (feature.Type != LunchFeatureType.Unknown)
            {
                feature.Line = node.Line;
                feature.LinePosition = node.LinePosition;
                feature.InnerText = node.InnerText;
                feature.OuterHtml = node.OuterHtml;
                feature.DetectedNode = node;
            }

            return feature;
        }


        private static FoodMatchType DetectFood(string food)
        {
            if (FoodKeywords.Any(knownFood => knownFood.Word.Equals(food)))
            {
                return FoodMatchType.Exact;
            }

            if (FoodKeywords.Any(knownFood => food.Contains(knownFood.Word)))
            {
                return FoodMatchType.Partial;
            }

            if (FoodKeywords.Any(knownFood => food.FuzzyMatchesFood(knownFood.Word)))
            {
                return FoodMatchType.Fuzzy;
            }

            return FoodMatchType.NoMatch;
        }


        private static bool FuzzyMatchesFood(this string unknownText, string knownFood)
        {
            var ls = new SimMetricsMetricUtilities.Levenstein();
            var similarity = ls.GetSimilarity(unknownText, knownFood);
            return similarity > 0.75;
        }
    }
}
