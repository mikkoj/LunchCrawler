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
            LunchMenuSeeker.SeekLunchMenus();
            /* LunchMenuSeeker.ScoreLunchMenu(@"http://blanko.net/cgi-bin/printtilounaslista.cgi");
            Separator();
            LunchMenuSeeker.ScoreLunchMenu(@"http://www.kupittaanpaviljonki.fi/lounaslista/"); */

            Console.ReadLine();
        }
    }
}
