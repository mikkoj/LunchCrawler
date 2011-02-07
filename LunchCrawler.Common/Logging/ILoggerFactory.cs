using System;


namespace LunchCrawler.Common.Logging
{
    public interface ILoggerFactory
    {
        ILogger Create(Type type);
        ILogger Create(string name);
        ILogger Create(Type type, LoggerLevel level);
        ILogger Create(string name, LoggerLevel level);
    }
}
