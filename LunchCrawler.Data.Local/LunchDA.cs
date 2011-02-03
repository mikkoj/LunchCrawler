using System.Linq;
using System.Data;
using System.Collections.Generic;


namespace Lunch.Data
{
    public class LunchDA
    {
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

        public IList<FoodKeyword> GetAllFoodKeywords()
        {
            using (var entityContext = new LunchEntities())
            {
                return entityContext.FoodKeywords.ToList();
            }
        }

        public IList<BasicFoodKeyword> GetAllBasicFoodKeywords()
        {
            using (var entityContext = new LunchEntities())
            {
                return entityContext.BasicFoodKeywords.ToList();
            }
        }
    }
}
