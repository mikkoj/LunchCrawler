using System;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;
using System.ComponentModel.Composition.Hosting;

using Autofac;
using Autofac.Integration.Mef;

using LunchCrawler.Common.Interfaces;
using LunchCrawler.Common.IoC;
using LunchCrawler.Common.Logging;


namespace LunchCrawler.MenuSeeker.Test
{
    class Program
    {
        static void Main()
        {
            var container = BuildComponentContainer();
            var lunchMenuSeeker = container.Resolve<ILunchMenuSeeker>();
            //container.InjectUnsetProperties(lunchMenuSeeker);
            lunchMenuSeeker.SeekLunchMenus();

            Console.ReadLine();
        }


        /// <summary>
        /// Uses dependency injection and MEF to build an Autofac container for the assembly.
        /// </summary>
        private static IContainer BuildComponentContainer()
        {
            // Autofac builder
            var builder = new ContainerBuilder();

            // NLogFactory will create ILoggers
            builder.RegisterModule(new LoggingInjectModule(new NLogFactory()));

            // let's create the catalog based on the types in the assembly
            var executingAssembly = Assembly.GetExecutingAssembly();
            var catalog = new AssemblyCatalog(executingAssembly);

            // let's register the MEF catalog
            builder.RegisterComposablePartCatalog(catalog);

            // we'll have to register the LunchMenuSeeker separately to inject the Logging properties
            builder.RegisterType<LunchMenuSeeker>().As<ILunchMenuSeeker>();

            // finally, let's build and return the container
            return builder.Build();
        }
    }
}
