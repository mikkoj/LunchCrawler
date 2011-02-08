using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac;
using Autofac.Core;

using LunchCrawler.Common.Logging;


namespace LunchCrawler.Common.IoC
{
    /// <summary>
    /// Adds cross-cutting logging injection to all registered types.
    /// All ILogger properties and parameters will be injected with an instance provided by the ILoggerFactory.
    /// </summary>
    public class LoggingInjectModule : Autofac.Module
    {
        private static ILoggerFactory _loggerFactory;

        /// <summary>
        /// When setupping the dependencies, an ILoggerFactory should be provided for this module.
        /// </summary>
        /// <param name="loggerFactory">
        /// Any factory that implements the ILoggerFactory interface and will provide instances of loggers.
        /// </param>
        public LoggingInjectModule(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        protected override void Load(ContainerBuilder moduleBuilder)
        {
            // will register the CreateLogger-delegate as response to a request for an ILogger implementation
            moduleBuilder.Register(CreateLogger).As<ILogger>().InstancePerDependency();
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry,
                                                              IComponentRegistration registration)
        {
            // hooks the Preparing event, which is fired whenever a new instance is required at construction time
            registration.Preparing += OnComponentPreparing;

            // let's get the type being registered
            var implementationType = registration.Activator.LimitType;

            // build an array of actions on this type to assign loggers to member properties
            var injectors = BuildLoggerInjectors(implementationType).ToList();

            // if there are no logger properties, there's no reason to hook the activated event
            if (!injectors.Any())
            {
                return;
            }

            // otherwise, whan an instance of this component is activated, inject the loggers on the instance
            registration.Activated += (s, e) =>
            {
                injectors.ForEach(injector => inje)
                foreach (var injector in injectors)
                {
                    injector(e.Context, e.Instance);
                }
            };
        }

        private static IEnumerable<Action<IComponentContext, object>> BuildLoggerInjectors(IReflect componentType)
        {
            // look for settable properties of type "ILogger"
            var loggerProperties = componentType
                .GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    PropertyInfo = p,
                    p.PropertyType,
                    IndexParameters = p.GetIndexParameters(),
                    Accessors = p.GetAccessors(false)
                })
                // must be a logger
                .Where(x => x.PropertyType == typeof(ILogger))
                // must not be an indexer
                .Where(x => x.IndexParameters.Count() == 0)
                // must have get/set, or only set
                .Where(x => x.Accessors.Length != 1 || x.Accessors[0].ReturnType == typeof(void));

            // return an IEnumerable of actions that resolve a logger and assign the property
            return loggerProperties
                   .Select(entry => entry.PropertyInfo)
                   .Select(propertyInfo => (Action<IComponentContext, object>)((ctx, instance) =>
                   {
                       var propertyValue = ctx.Resolve<ILogger>(new TypedParameter(typeof(Type), componentType));
                       propertyInfo.SetValue(instance, propertyValue, null);
                   }));
        }

        private static void OnComponentPreparing(object sender, PreparingEventArgs e)
        {
            // resolves all ILogger type parameters on constructors or functions
            var componentType = e.Component.Activator.LimitType;
            e.Parameters = e.Parameters.Union(new[]
            {
                new ResolvedParameter((p, ctx) => p.ParameterType == typeof(ILogger),
                                      (p, ctx) => ctx.Resolve<ILogger>(new TypedParameter(typeof(Type), componentType)))
            });
        }

        private static ILogger CreateLogger(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            // return an ILogger in response to Resolve<ILogger>(componentTypeParameter)
            var containingType = parameters.TypedAs<Type>();
            return _loggerFactory.Create(containingType);
        }
    }
}
