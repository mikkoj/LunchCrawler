using System;

using HtmlAgilityPack;

using LunchCrawler.Common.Interfaces;
using LunchCrawler.Common.Model;


namespace LunchCrawler.Analyzer.Test.Strategies
{
    /// <summary>
    /// The strict 5-day strategy assumes that there is a food item for every
    /// weekday in the lunch menu, along with prices and restaurant info.
    /// </summary>
    public class StrictFiveDayStrategy : ILunchMenuParsingStrategy
    {
        public int Priority
        {
            get { return 10; }
        }

        public LunchMenu ParseDocument(HtmlDocument document)
        {
            var lunchMenu = new LunchMenu { ParsingStrategy = this };

            // 1. let's collect and print the basic features for the document
            var features = SimpleFeatureDetector.DetectFeatures(document);
            SimpleFeatureDetector.PrintDetectedFeatures(features);

            var requiredWeekDays = new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday
            };

            // 2. calculate distances and number of food items per day

            // 3. find prices

            return lunchMenu;
        }
    }
}
