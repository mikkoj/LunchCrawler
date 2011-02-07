using System;
using System.IO;

using NLog;
using NLog.Config;


namespace LunchCrawler.Common.Logging
{
    public class NLogFactory : ILoggerFactory
    {
        public NLogFactory() : this("nlog.config")
        {
        }

        public NLogFactory(string configFile)
        {
            LogManager.Configuration = new XmlLoggingConfiguration(GetConfigFile(configFile).FullName);
        }

        public ILogger Create(string name)
        {
            return new NLogLogger(LogManager.GetLogger(name), this);
        }

        public ILogger Create(string name, LoggerLevel level)
        {
            throw new NotImplementedException("Logger levels cannot be set at runtime. Please review your configuration file.");
        }

        public ILogger Create(Type type)
        {
            return new NLogLogger(LogManager.GetLogger(type.Name), this);
        }

        public ILogger Create(Type type, LoggerLevel level)
        {
            throw new NotImplementedException();
        }

        private static FileInfo GetConfigFile(string configFile)
        {
            if (Path.IsPathRooted(configFile))
            {
                return new FileInfo(configFile);
            }
            return new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile));

        }
    }
}
