using System.Reflection;

using Autofac;

using LunchCrawler.Common.Logging;


namespace LunchCrawler.Common.IoC
{
    /// <summary>
    /// Acts as a service locator singleton.
    /// To be used mainly for building the container in the first place,
    /// not for resolving other than specific instances.
    /// </summary>
    public sealed class ServiceLocator
    {
        static readonly ServiceLocator LocatorInstance = new ServiceLocator();
        private IContainer _container;

        static ServiceLocator() { }
        ServiceLocator() { }

        /// <summary>
        /// Singleton instance for IoC-container.
        /// </summary>
        public static ServiceLocator Instance
        {
            get { return LocatorInstance; }
        }

        /// <summary>
        /// Returns an Autofac container for the assembly.
        /// </summary>
        public IContainer Container
        {
            get
            {
                if (_container == null)
                {
                    // Autofac builder
                    var builder = new ContainerBuilder();

                    // NLogFactory will create ILoggers
                    builder.RegisterModule(new LoggingInjectModule(new NLogFactory()));

                    // let's create the catalog based on the types in the calling assembly
                    var assembly = Assembly.GetCallingAssembly();
                    builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();

                    // finally, let's build and return the container
                    _container = builder.Build();
                }
                return _container;
            }
        }
    }
}
