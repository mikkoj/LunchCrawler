using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;


namespace LunchCrawler.Data.Local
{
    public sealed class LunchDA
    {
        static readonly LunchDA _instance = new LunchDA();

        static LunchDA() { }
        LunchDA() { }

        public static LunchDA Instance
        {
            get { return _instance; }
        }

        public void AddFoodKeyword(string food)
        {
            try
            {
                using (var entityContext = new LunchEntities())
                {
                    var foodKeyword = new FoodKeyword
                    {
                        Word = food
                    };

                    entityContext.FoodKeywords.AddObject(foodKeyword);
                    entityContext.SaveChanges();
                }
            }
            catch (UpdateException updateException)
            {
                var baseError = updateException.GetBaseException();
                if (baseError.Message.Contains("not unique"))
                {
                    return;
                }

                throw;
            }
        }

        public void UpdateLunchMenuKeywordDetectionCounts(IDictionary<string, int> keywordCounts)
        {
            using (var entityContext = new LunchEntities())
            {
                foreach (var keywordPair in keywordCounts)
                {
                    var keywordName = keywordPair.Key;
                    var keywordCount = keywordPair.Value;

                    var existingKeyword = entityContext.LunchMenuKeywords
                                                       .FirstOrDefault(keyword => keyword.Word.Equals(keywordName,
                                                                                                      StringComparison.InvariantCultureIgnoreCase));
                    if (existingKeyword == null)
                    {
                        continue;
                    }

                    var existingDetectionCount = existingKeyword.DetectionCount;
                    var newDetectionCount = existingDetectionCount + keywordCount;

                    existingKeyword.DetectionCount = newDetectionCount;
                }

                entityContext.SaveChanges();
            }
        }


        public IList<FoodKeyword> GetAllFoodKeywords()
        {
            using (var entityContext = new LunchEntities())
            {
                return entityContext.FoodKeywords.ToList();
            }
        }

        public IEnumerable<LunchMenuKeyword> GetAllBasicLunchMenuKeywords()
        {
            using (var entityContext = new LunchEntities())
            {
                return entityContext.LunchMenuKeywords.ToList();
            }
        }

        public IList<SearchKeyword> GetAllSearchKeywords()
        {
            using (var entityContext = new LunchEntities())
            {
                return entityContext.SearchKeywords.ToList();
            }
        }

        /// <summary>
        /// Get lunch restaurants with a given status and minimum probability.
        /// </summary>
        public IList<LunchRestaurant> GetLunchRestaurants(int status, decimal minProbability)
        {
            using (var entityContext = new LunchEntities())
            {
                return entityContext.LunchRestaurants.Where(rt => rt.Status == status &&
                                                                  rt.LunchMenuProbability > minProbability).ToList();
            }
        }


        public IList<LunchRestaurant> GetLunchRestaurants(int status)
        {
            using (var entityContext = new LunchEntities())
            {
                return entityContext.LunchRestaurants.Where(rt => rt.Status == status).ToList();
            }
        }

        public IList<LunchRestaurant> GetLunchRestaurants(params int[] states)
        {
            using (var entityContext = new LunchEntities())
            {
                return entityContext.LunchRestaurants
                                    .Where(rt => states.Any(status => status == rt.Status))
                                    .ToList();
            }
        }

        /// <summary>
        /// Searches the DB for an existing lunch restaurant.
        /// </summary>
        /// <param name="lunchRestaurantUrl">An existing lunch restaurant URL.</param>
        public LunchRestaurant FindExistingLunchRestaurant(string lunchRestaurantUrl)
        {
            using (var entityContext = new LunchEntities())
            {
                return entityContext.LunchRestaurants
                                    .FirstOrDefault(rt => rt.URL.Equals(lunchRestaurantUrl, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        /// <summary>
        /// Adds a new lunch restaurant or updates an existing one.
        /// </summary>
        public void UpdateLunchRestaurant(LunchRestaurant lunchRestaurant)
        {
            try
            {
                using (var entityContext = new LunchEntities())
                {
                    var existingUrl = FindExistingLunchRestaurant(lunchRestaurant.URL);
                    if (existingUrl != null)
                    {
                        existingUrl.Status = lunchRestaurant.Status;
                        existingUrl.SiteHash = lunchRestaurant.SiteHash;
                        existingUrl.LunchMenuProbability = lunchRestaurant.LunchMenuProbability;
                        existingUrl.TotalPoints = lunchRestaurant.TotalPoints;
                        existingUrl.TotalKeywordDetections = lunchRestaurant.TotalKeywordDetections;
                        existingUrl.ExactKeywordDetections = lunchRestaurant.ExactKeywordDetections;
                        existingUrl.PartialKeywordDetections = lunchRestaurant.PartialKeywordDetections;
                        existingUrl.FuzzyKeywordDetections = lunchRestaurant.FuzzyKeywordDetections;
                        existingUrl.DateUpdated = DateTime.UtcNow;
                    }
                    else
                    {
                        lunchRestaurant.DateAdded = DateTime.UtcNow;
                        entityContext.LunchRestaurants.AddObject(lunchRestaurant);
                    }

                    entityContext.SaveChanges();
                }
            }
            catch (UpdateException updateException)
            {
                var baseError = updateException.GetBaseException();
                if (baseError.Message.Contains("not unique"))
                {
                    return;
                }

                throw;
            }
        }

        public LunchMenuKeyword GetLunchMenuKeyword(string word)
        {
            try
            {
                using (var entityContext = new LunchEntities())
                {
                    return entityContext.LunchMenuKeywords
                                        .FirstOrDefault(keyword => keyword.Word.Equals(word, StringComparison.InvariantCultureIgnoreCase));
                }
            }
            catch (UpdateException updateException)
            {
                var baseError = updateException.GetBaseException();
                if (baseError.Message.Contains("not unique"))
                {
                    return null;
                }

                throw;
            }
        }
    }
}
