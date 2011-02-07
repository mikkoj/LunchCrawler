using System;


namespace LunchCrawler.Common.Logging
{
    public interface ILogger
    {
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }

        [Obsolete("Use IsFatalEnabled instead")]
        bool IsFatalErrorEnabled { get; }

        void Debug(string message);
        void Debug(string message, Exception exception);
        void Debug(string format, params object[] args);
        void DebugFormat(string format, params object[] args);
        void DebugFormat(Exception exception, string format, params object[] args);
        void DebugFormat(IFormatProvider formatProvider, string format, params object[] args);
        void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);
        void Info(string message);
        void Info(string message, Exception exception);
        void Info(string format, params object[] args);
        void InfoFormat(string format, params object[] args);
        void InfoFormat(Exception exception, string format, params object[] args);
        void InfoFormat(IFormatProvider formatProvider, string format, params object[] args);
        void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);
        void Warn(string message);
        void Warn(string message, Exception exception);
        void Warn(string format, params object[] args);
        void WarnFormat(string format, params object[] args);
        void WarnFormat(Exception exception, string format, params object[] args);
        void WarnFormat(IFormatProvider formatProvider, string format, params object[] args);
        void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);
        void Error(string message);
        void Error(string message, Exception exception);
        void Error(string format, params object[] args);
        void ErrorFormat(string format, params object[] args);
        void ErrorFormat(Exception exception, string format, params object[] args);
        void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args);
        void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);
        void Fatal(string message);
        void Fatal(string message, Exception exception);
        void Fatal(string format, params object[] args);
        void FatalFormat(string format, params object[] args);
        void FatalFormat(Exception exception, string format, params object[] args);
        void FatalFormat(IFormatProvider formatProvider, string format, params object[] args);
        void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);

        [Obsolete("Use Fatal instead")]
        void FatalError(string message);

        [Obsolete("Use Fatal instead")]
        void FatalError(string message, Exception exception);

        [Obsolete("Use Fatal or FatalFormat instead")]
        void FatalError(string format, params object[] args);

        ILogger CreateChildLogger(string loggerName);
    }
}
