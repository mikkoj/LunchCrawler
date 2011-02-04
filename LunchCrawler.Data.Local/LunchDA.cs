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
    }
}
