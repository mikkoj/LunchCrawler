using System.IO;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using LunchCrawler.Data.Local;
using LunchCrawler.Common.Enums;


namespace LunchCrawler.Analyzer.Test
{
    public static class LunchMenuDetection
    {
        private static readonly IList<string> WeekDays = File.ReadAllLines("WeekDays.txt").ToList();
        private const string WeekPattern = @"(viikko|vko|week) (\d+)";
        private const string MoneyPattern = @"\d+([.,]\d+)?\s*?[€e]|[^.](\d+[.,]\d\d)";

        private static readonly IList<string> SkipPatterns = File.ReadAllLines("SkipPatterns.txt").ToList();
        private static readonly IList<FoodKeyword> FoodKeywords = LunchDA.Instance.GetAllFoodKeywords();


        public static LunchMenuFeature DetectFeature(HtmlNode node)
        {
            var feature = new LunchMenuFeature
            {
                Type = LunchMenuFeatureType.Unknown,
                FoodMatchType = StringMatchType.NoMatch
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
                feature.Type = LunchMenuFeatureType.Unknown;
            }

            // try to detect weekdays and week
            if (WeekDays.Any(d => d.Equals(nodeText)))
            {
                feature.Type = LunchMenuFeatureType.Weekday;
            }

            if (Regex.IsMatch(nodeText, WeekPattern))
            {
                feature.Type = LunchMenuFeatureType.Week;
            }

            // -----------------------------------------------------------------------------------------------------
            // try to detect food and money

            feature.FoodMatchType = DetectFood(nodeText);
            var foodDetected = feature.FoodMatchType != StringMatchType.NoMatch;

            var moneyDetected = Regex.IsMatch(nodeText, MoneyPattern);
            
            if (foodDetected && moneyDetected)
            {
                feature.Type = LunchMenuFeatureType.FoodAndMoney;
            }

            if (foodDetected)
            {
                feature.Type = LunchMenuFeatureType.Food;
            }

            if (moneyDetected)
            {
                feature.Type = LunchMenuFeatureType.Money;
            }


            // only complete the feature if it was recognized
            if (feature.Type != LunchMenuFeatureType.Unknown)
            {
                feature.Line = node.Line;
                feature.LinePosition = node.LinePosition;
                feature.InnerText = node.InnerText;
                //feature.OuterHtml = node.OuterHtml;
                feature.DetectedNode = node;
            }

            return feature;
        }


        private static StringMatchType DetectFood(string food)
        {
            if (FoodKeywords.Any(knownFood => knownFood.Word.Equals(food)))
            {
                return StringMatchType.Exact;
            }

            if (FoodKeywords.Any(knownFood => food.Contains(knownFood.Word)))
            {
                return StringMatchType.Partial;
            }

            if (FoodKeywords.Any(knownFood => food.FuzzyMatchesFood(knownFood.Word)))
            {
                return StringMatchType.Fuzzy;
            }

            return StringMatchType.NoMatch;
        }


        private static bool FuzzyMatchesFood(this string unknownText, string knownFood)
        {
            var ls = new SimMetricsMetricUtilities.Levenstein();
            var similarity = ls.GetSimilarity(unknownText, knownFood);
            return similarity > 0.75;
        }
    }
}
