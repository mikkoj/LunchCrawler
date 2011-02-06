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

        public void UpdateLunchMenuKeywordDetectionCount(string word, long detectionCount)
        {
            try
            {
                using (var entityContext = new LunchEntities())
                {
                    var existingKeyword = entityContext.LunchMenuKeywords
                                                       .FirstOrDefault(keyword => keyword.Word.Equals(word));
                    if (existingKeyword != null)
                    {
                        existingKeyword.DetectionCount = detectionCount;
                        entityContext.SaveChanges();
                    }
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

        public IList<FoodKeyword> GetAllFoodKeywords()
        {
            using (var entityContext = new LunchEntities())
            {
                return entityContext.FoodKeywords.ToList();
            }
        }

        public IList<LunchMenuKeyword> GetAllBasicLunchMenuKeywords()
        {
            using (var entityContext = new LunchEntities())
            {
                return entityContext.LunchMenuKeywords.ToList();
            }
        }

        public void UpdateWithPotentialLunchMenu(string url, PotentialLunchMenu lunchMenu)
        {
            try
            {
                using (var entityContext = new LunchEntities())
                {
                    var existingUrl = entityContext.PotentialLunchMenus
                                                   .FirstOrDefault(menu => menu.URL.Equals(url));
                    if (existingUrl != null)
                    {
                        existingUrl.Status = lunchMenu.Status;
                        
                        existingUrl.SiteHash = lunchMenu.SiteHash;
                        existingUrl.LunchMenuProbability = lunchMenu.LunchMenuProbability;
                        existingUrl.TotalPoints = lunchMenu.TotalPoints;
                        existingUrl.TotalKeywordDetections = lunchMenu.TotalKeywordDetections;
                        existingUrl.ExactKeywordDetections = lunchMenu.ExactKeywordDetections;
                        existingUrl.PartialKeywordDetections = lunchMenu.PartialKeywordDetections;
                        existingUrl.FuzzyKeywordDetections = lunchMenu.FuzzyKeywordDetections;
                        existingUrl.DateUpdated = DateTime.UtcNow;
                    }
                    else
                    {
                        lunchMenu.DateAdded = DateTime.UtcNow;
                        entityContext.PotentialLunchMenus.AddObject(lunchMenu);
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
                                        .FirstOrDefault(keyword => keyword.Word.Equals(word));
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
