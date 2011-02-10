using System;


namespace LunchCrawler.Analyzer.Test
{
    class Program
    {
        static void Main()
        {
            var parser = new LunchRestaurantAnalyzer();

            parser.AnalyzeLunchRestaurant(@"http://blanko.net/cgi-bin/printtilounaslista.cgi");
            Separator();
            parser.AnalyzeLunchRestaurant(@"http://www.veritasstadion.com/cgi/menu.htm");
            Separator();
            parser.AnalyzeLunchRestaurant(@"http://www.kupittaanpaviljonki.fi/lounaslista/");

            Console.ReadLine();
        }

        private static void Separator()
        {
            Console.WriteLine("\n\n--------------------------------------------------------------\n");
        }
    }
}
