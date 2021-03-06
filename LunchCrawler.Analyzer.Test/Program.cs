﻿using System;
using System.Diagnostics;
using System.Reflection;

using Autofac;

using LunchCrawler.Common;
using LunchCrawler.Common.IoC;
using LunchCrawler.Common.Logging;


namespace LunchCrawler.Analyzer.Test
{
    class Program
    {
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            var container = BuildComponentContainer();
            var lunchRestaurantAnalyzer = container.Resolve<LunchRestaurantAnalyzer>();

            Console.WriteLine("-> Started analyzing lunch restaurants..");
            var watch = new Stopwatch();
            watch.Start();

            lunchRestaurantAnalyzer.Start();

            watch.Stop();

            Console.WriteLine("\n\nLunch menu analyzing done in {0}", watch.Elapsed);
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

            Console.WriteLine("Unknown error: " + ex.ParseInnerException());
        }
    }
}
