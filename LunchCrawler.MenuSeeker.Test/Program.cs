using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

using Autofac;

using LunchCrawler.Common.IoC;
using LunchCrawler.Common.Logging;
using LunchCrawler.Common.Interfaces;


namespace LunchCrawler.MenuSeeker.Test
{
    class Program
    {
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            var container = BuildComponentContainer();
            var lunchMenuSeeker = container.Resolve<ILunchMenuSeeker>();

            Console.WriteLine("-> Started seeking lunch menus..");
            var watch = new Stopwatch();
            watch.Start();

            try
            {
                const int maxStackSize = 10485760; // 10MB
                var task = new ThreadStart(lunchMenuSeeker.SeekLunchMenus);
                var thread = new Thread(task, maxStackSize);
                thread.Start();
            }
            catch (OutOfMemoryException ex)
            {
                // ..
            }
            catch (Exception ex)
            {
                // ..
            }

            //lunchMenuSeeker.SeekLunchMenus();

            watch.Stop();
            
            Console.WriteLine("\n\nLunch menu seeking done in {0}", watch.Elapsed);
            Console.ReadLine();
        }


        /// <summary>
        /// Uses dependency injection to build an Autofac container for the assembly.
        /// </summary>
        private static IContainer BuildComponentContainer()
        {
            // Autofac builder
            var builder = new ContainerBuilder();

            // NLogFactory will create ILoggers
            builder.RegisterModule(new LoggingInjectModule(new NLogFactory()));

            // let's create the catalog based on the types in the assembly
            var assembly = Assembly.GetExecutingAssembly();
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();

            // finally, let's build and return the container
            return builder.Build();
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
