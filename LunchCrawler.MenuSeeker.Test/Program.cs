using System;
using System.Diagnostics;

using Autofac;

using LunchCrawler.Common.Interfaces;
using LunchCrawler.Common.IoC;


namespace LunchCrawler.MenuSeeker.Test
{
    class Program
    {
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            var container = ServiceLocator.Instance.Container;
            var lunchRestaurantSeeker = container.Resolve<ILunchRestaurantSeeker>();

            Console.WriteLine("-> Started seeking lunch menus..");
            var watch = new Stopwatch();
            watch.Start();

            lunchRestaurantSeeker.Start();

            watch.Stop();
            
            Console.WriteLine("\n\nLunch menu seeking done in {0}", watch.Elapsed);
            Console.ReadLine();
        }


        static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex == null)
            {
                Console.WriteLine("Unknown error occurred");
                return;
            }

            Console.WriteLine("Unknown error: " + ex.Message);
        }
    }
}
