using System.Collections.Generic;
using System.Text.RegularExpressions;

using LunchCrawler.Common;
using LunchCrawler.Common.Enums;
using LunchCrawler.Common.Model;
using LunchCrawler.Data.Local;

using LunchMenuFeature = LunchCrawler.Common.Model.LunchMenuFeature;


namespace LunchCrawler.Analyzer.Test.Strategies
{
    /// <summary>
    /// Base class for all strategies with some basic functionality.
    /// </summary>
    public abstract class ParsingStrategyBase
    {
        public static bool ShouldSkipAnalysis(LunchRestaurantDocument doc)
        {
            var existingDoc = LunchDA.Instance.FindExistingLunchRestaurant(doc.URL);
            return existingDoc != null && existingDoc.SiteHash.Equals(doc.Hash);
        }

        /// <summary>
        /// Adds a food item to a lunch menu for a given weekday.
        /// </summary>
        public static void AddFoodItemForWeekDay(LunchMenu lunchMenu,
                                                 WeekDay currentWeekDay,
                                                 LunchMenuFeature foodFeature,
                                                 LunchMenuFeature priceFeature = null)
        {
            var lunchMenuFoodItem = new LunchMenuFoodItem
            {
                FoodItem = Utils.HtmlDecode(foodFeature.InnerText).Trim()
            };

            if (priceFeature != null)
            {
                lunchMenuFoodItem.Price = ParsePrice(priceFeature.InnerText);
            }
            else
            {
                // TODO: parse food item and price from the same string
            }

            if (lunchMenu.FoodItems.ContainsKey(currentWeekDay))
            {
                lunchMenu.FoodItems[currentWeekDay].Add(lunchMenuFoodItem);
            }
            else
            {
                lunchMenu.FoodItems.Add(currentWeekDay, new List<LunchMenuFoodItem>
                {
                    lunchMenuFoodItem
                });
            }
        }


        /// <summary>
        /// Attempts to parse the price for a detected price feature.
        /// </summary>
        private static decimal? ParsePrice(string priceText)
        {
            const string pricePattern = @"([1-9][0-9]*[.,]\d\d?)\s*?([€eE]|euro)";
            // TODO: 'anti-pattern'-check päivämäärille (?)
            // const string notPricePattern = 

            try
            {
                var match = Regex.Match(priceText.Trim(), pricePattern);
                if (match.Success)
                {
                    decimal price;
                    if (decimal.TryParse(match.Groups[1].Value, out price))
                    {
                        return price;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
