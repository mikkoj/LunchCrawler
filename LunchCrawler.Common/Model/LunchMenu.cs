using System.Collections;
using System.Collections.Generic;

using LunchCrawler.Common.Enums;


namespace LunchCrawler.Common.Model
{
    /// <summary>
    /// Represents a parsing result for a lunch menu.
    /// </summary>
    public class LunchMenu
    {
        /// <summary>
        /// Food items for given days of the week.
        /// </summary>
        public IDictionary<WeekDay, List<LunchMenuFoodItem>> FoodItems;

        public LunchMenu()
        {
            FoodItems = new Dictionary<WeekDay, List<LunchMenuFoodItem>>();
        }

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
    }
}
