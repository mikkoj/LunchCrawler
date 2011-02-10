using System;
using System.Collections.Generic;

using LunchCrawler.Common.Interfaces;


namespace LunchCrawler.Common.Model
{
    /// <summary>
    /// Represents a parsing result for a lunch menu.
    /// </summary>
    public class LunchMenu
    {
        /// <summary>
        /// Parsing strategy used when parsing the lunch menu.
        /// </summary>
        public ILunchMenuParsingStrategy ParsingStrategy { get; set; }

        /// <summary>
        /// Confidence of parsing success (0 - 1).
        /// </summary>
        public decimal Confidence { get; set; }

        /// <summary>
        /// The restaurant the menu is for (URL for now).
        /// </summary>
        public string RestaurantKey { get; set; }

        /// <summary>
        /// Week number the menu is for.
        /// </summary>
        public int? WeekNumber { get; set; }

        /// <summary>
        /// Food items for given days of the week.
        /// </summary>
        public IDictionary<DayOfWeek, LunchMenuFoodItem> FoodItems { get; set; }
    }
}
