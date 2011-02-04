using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunchCrawler.MenuSeeker.Test
{
    class Program
    {
        static void Main()
        {
            var parser = new LunchMenuSeeker();

            parser.ScoreLunchMenu(@"http://blanko.net/cgi-bin/printtilounaslista.cgi");
            Separator();
            parser.ScoreLunchMenu(@"http://www.veritasstadion.com/cgi/menu.htm");
            Separator();
            parser.ScoreLunchMenu(@"http://www.kupittaanpaviljonki.fi/lounaslista/");

            Console.ReadLine();
        }

        private static void Separator()
        {
            Console.WriteLine("\n\n--------------------------------------------------------------\n");
        }
    }
}
