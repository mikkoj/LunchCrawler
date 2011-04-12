using LunchCrawler.Common.Enums;
using LunchCrawler.Data.Local;

using NUnit.Framework;

namespace EntityFrameworkTest
{
    [TestFixture]
    public class EntityFrameworkMockTesti
    {
        [Test]
        public void HaeRavintolatKannasta()
        {
            var kantaRepository = new LunchRestaurantRepository();
            var ravintolat = kantaRepository.AnnaRavintolatStatuksella((int)LunchRestaurantStatus.OK);
            Assert.That(ravintolat.Count > 0);
        }


        [Test]
        public void HaeRavintolatMockattuna()
        {
            var testiKonteksti = new MockKonteksti();
            
            // luodaan feikki"kantaan" yksi ravintola
            testiKonteksti.LunchRestaurants.AddObject(new LunchRestaurant
            {
                Status = (int)LunchRestaurantStatus.OK,
                AbsoluteURL = "www.awesome.fi"
            });

            var kantaRepository = new LunchRestaurantRepository(testiKonteksti);
            var ravintolat = kantaRepository.AnnaRavintolatStatuksella((int)LunchRestaurantStatus.OK);
            Assert.That(ravintolat.Count > 0);
        }
    }
}
