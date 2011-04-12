using System.Data.Objects;

using LunchCrawler.Data.Local;


namespace EntityFrameworkTest
{
    /// <summary>
    /// Konteksti testausta varten.
    /// </summary>
    public class MockKonteksti : IContext
    {
        private IObjectSet<LunchRestaurant> _lunchRestaurants;
        private IObjectSet<FoodKeyword> _foodKeywords;
        private IObjectSet<LunchMenuFeature> _lunchMenuFeatures;
        private IObjectSet<LunchMenuKeyword> _lunchMenuKeywords;
        private IObjectSet<SearchKeyword> _searchKeywords;



        public IObjectSet<FoodKeyword> FoodKeywords
        {
            get 
            {
                if (_foodKeywords == null)
                {
                    _foodKeywords = new MockObjectSet<FoodKeyword>();
                }
                return _foodKeywords;
            }
            set { _foodKeywords = value; }
        }


        public IObjectSet<LunchMenuFeature> LunchMenuFeatures
        {
            get
            {
                if (_lunchMenuFeatures == null)
                {
                    _lunchMenuFeatures = new MockObjectSet<LunchMenuFeature>();
                }
                return _lunchMenuFeatures;
            }
            set { _lunchMenuFeatures = value; }
        }


        public IObjectSet<LunchMenuKeyword> LunchMenuKeywords
        {
            get
            {
                if (_lunchMenuKeywords == null)
                {
                    _lunchMenuKeywords = new MockObjectSet<LunchMenuKeyword>();
                }
                return _lunchMenuKeywords;
            }
            set { _lunchMenuKeywords = value; }
        }


        public IObjectSet<SearchKeyword> SearchKeywords
        {
            get
            {
                if (_searchKeywords == null)
                {
                    _searchKeywords = new MockObjectSet<SearchKeyword>();
                }
                return _searchKeywords;
            }
            set { _searchKeywords = value; }
        }


        public IObjectSet<LunchRestaurant> LunchRestaurants
        {
            get
            {
                if (_lunchRestaurants == null)
                {
                    _lunchRestaurants = new MockObjectSet<LunchRestaurant>();
                }
                return _lunchRestaurants;
            }
            set { _lunchRestaurants = value; }
        }
    }
}
