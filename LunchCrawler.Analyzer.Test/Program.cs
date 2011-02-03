using System;


namespace LunchCrawler.Analyzer.Test
{
    class Program
    {
        static void Main()
        {
            var parser = new LunchMenuParser();

            parser.ParseLunchMenu(@"http://blanko.net/cgi-bin/printtilounaslista.cgi");
            Separator();
            parser.ParseLunchMenu(@"http://www.veritasstadion.com/cgi/menu.htm");
            Separator();
            parser.ParseLunchMenu(@"http://www.kupittaanpaviljonki.fi/lounaslista/");

            Console.ReadLine();
        }

        private static void Separator()
        {
            Console.WriteLine("\n\n--------------------------------------------------------------\n");
        }
    }
}
