using System;
using System.Linq;
using System.Text;

using LunchCrawler.Common;
using LunchCrawler.Common.Interfaces;
using LunchCrawler.Common.Logging;
using LunchCrawler.Common.Model;
using LunchCrawler.Common.Enums;
using LunchCrawler.Data.Local;

using LunchMenuFeature = LunchCrawler.Common.Model.LunchMenuFeature;


namespace LunchCrawler.Analyzer.Test.Strategies
{
    /// <summary>
    /// The strict 5-day strategy assumes that there's a food item for every
    /// weekday in the lunch menu, along with price and restaurant info.
    /// </summary>
    public class StrictFiveDayStrategy : ParsingStrategyBase, ILunchMenuParsingStrategy
    {
        public int Priority
        {
            get { return 10; }
        }

        public ILogger Logger { get; set; }

        public StrictFiveDayStrategy()
        {
            Logger = NullLogger.Instance;
        }


        private static readonly string[] RequiredWeekdays = new[]
        {
            "maanantai", "tiistai", "keskiviikko", "torstai", "perjantai"
        };

        public LunchMenu ParseLunchMenu(LunchRestaurant restaurant)
        {
            var lunchMenu = new LunchMenu { RestaurantKey = restaurant.URL, Confidence = 0 };

            var doc = Utils.GetLunchRestaurantDocumentForUrl(restaurant.AbsoluteURL);
            if (ShouldSkipAnalysis(doc))
            {
                return lunchMenu;
            }

            var nodeCount = doc.HtmlDocument.DocumentNode.DescendantNodes().Count();
            
            // TODO: yliraja pisteille / noodeille jo Seeker puolelle - jumittaa muuten strategioissa
            if (nodeCount > 7000)
            {
                return lunchMenu;
            }

            // 1. first, let's collect and print the basic features for the document
            var features = SimpleFeatureDetector.DetectFeatures(doc.HtmlDocument)
                                                .Where(f => f.Type != LunchMenuFeatureType.Unknown)
                                                .ToList();

            // SimpleFeatureDetector.PrintDetectedFeatures(features);
            

            // 2. let's make sure all 5 weekdays are found
            if (!RequiredWeekdays.All(day => features.Any(f => f.Type == LunchMenuFeatureType.Weekday &&
                                                               f.InnerText.ToLowerInvariant().Contains(day))))
            {
                // we shouldn't continue with this strategy
                return lunchMenu;
            }


            // 3. let's find the index for the first weekday
            var firstWeekdayIndex = features.FindIndex(f => f.Type == LunchMenuFeatureType.Weekday &&
                                                            f.InnerText.ToLowerInvariant().Contains("maanantai"));
            if (firstWeekdayIndex == -1)
            {
                return lunchMenu;
            }


            // 4. let's loop through the detected features and collect food items for each weekday
            var currentWeekDay = WeekDay.Monday;
            for (var i = (firstWeekdayIndex + 1); i < features.Count; i++)
            {
                var currentFeature = features[i];
                LunchMenuFeature nextFeature = null;
                if (features.Count != (i + 1))
                {
                    nextFeature = features[i + 1];
                }

                if (currentFeature.Type == LunchMenuFeatureType.FoodItemAndPrice)
                {
                    AddFoodItemForWeekDay(lunchMenu, currentWeekDay, currentFeature);
                }
                else if (currentFeature.Type == LunchMenuFeatureType.FoodItem &&
                         nextFeature != null && nextFeature.Type == LunchMenuFeatureType.Price)
                {
                    AddFoodItemForWeekDay(lunchMenu, currentWeekDay, currentFeature, nextFeature);
                }
                else if (currentFeature.Type == LunchMenuFeatureType.Weekday)
                {
                    // we'll only accept detected weekday as the next weekday if weekdays are in correct order
                    var detectedWeekday = ParseWeekDay(currentFeature.InnerText);
                    var nextWeekday = (WeekDay)Enum.ToObject(typeof(WeekDay), (int)currentWeekDay + 1);

                    if (detectedWeekday != nextWeekday &&
                        (lunchMenu.FoodItems == null || lunchMenu.FoodItems.Keys.Count < 5))
                    {
                        return lunchMenu;
                    }

                    currentWeekDay = detectedWeekday;
                }
            }

            // 5. finally, let's make sure we have atleast some food items for each day
            var detectedDays = lunchMenu.FoodItems.Keys.Count;
            var foodItemsForEachWeekday = lunchMenu.FoodItems.GroupBy(f => f.Key).All(g => g.Count() > 0);

            if (detectedDays >= 5 && foodItemsForEachWeekday)
            {
                lunchMenu.Confidence = 1;
            }

            if (lunchMenu.FoodItems.Count > 0)
            {
                var result = new StringBuilder("results for {0}:\n".With(restaurant.AbsoluteURL));
                foreach (var weekDayItems in lunchMenu.FoodItems.GroupBy(f => f.Key).Where(g => g.Any()))
                {
                    result.AppendFormat("-> {0} - {1} food items\n", weekDayItems.Key, weekDayItems.Count());
                }

                Logger.Info(result.ToString());
            }
            else
            {
                Logger.InfoFormat("{0} - no results.\n", restaurant.AbsoluteURL);
            }

            return lunchMenu;
        }


        /// <summary>
        /// TODO: WeekDay-enum attribuutiksi arvot tai regexpit (?)
        /// </summary>
        /// <param name="weekday"></param>
        /// <returns></returns>
        private static WeekDay ParseWeekDay(string weekday)
        {
            switch (weekday.ToLowerInvariant().Trim())
            {
                case "maanantai":   return WeekDay.Monday;
                case "tiistai":     return WeekDay.Tuesday;
                case "keskiviikko": return WeekDay.Wednesday;
                case "torstai":     return WeekDay.Thursday;
                case "perjantai":   return WeekDay.Friday;
            }

            return WeekDay.Unknown;
        }
    }
}
