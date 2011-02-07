using System;


namespace LunchCrawler.Common.Logging
{
    /// <summary>
    /// NullLogger will act as a dummy logger, and will 'swallow' all logging commands.
    /// </summary>
    public class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new NullLogger();

        public ILogger CreateChildLogger(string loggerName)
        {
            return this;
        }

        public void Debug(string message)
        {
        }

        public void Debug(string format, params object[] args)
        {
        }

        public void Debug(string message, Exception exception)
        {
        }

        public void DebugFormat(string format, params object[] args)
        {
        }

        public void DebugFormat(Exception exception, string format, params object[] args)
        {
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void Error(string message)
        {
        }

        public void Error(string message, Exception exception)
        {
        }

        public void Error(string format, params object[] args)
        {
        }

        public void ErrorFormat(string format, params object[] args)
        {
        }

        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void Fatal(string message)
        {
        }

        public void Fatal(string message, Exception exception)
        {
        }

        public void Fatal(string format, params object[] args)
        {
        }

        [Obsolete("Use Fatal instead")]
        public void FatalError(string message)
        {
        }

        [Obsolete("Use Fatal or FatalFormat instead")]
        public void FatalError(string format, params object[] args)
        {
        }

        [Obsolete("Use Fatal instead")]
        public void FatalError(string message, Exception exception)
        {
        }

        public void FatalFormat(string format, params object[] args)
        {
        }

        public void FatalFormat(Exception exception, string format, params object[] args)
        {
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void Info(string message)
        {
        }

        public void Info(string message, Exception exception)
        {
        }

        public void Info(string format, params object[] args)
        {
        }

        public void InfoFormat(string format, params object[] args)
        {
        }

        public void InfoFormat(Exception exception, string format, params object[] args)
        {
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void Warn(string message)
        {
        }

        public void Warn(string message, Exception exception)
        {
        }

        public void Warn(string format, params object[] args)
        {
        }

        public void WarnFormat(string format, params object[] args)
        {
        }

        public void WarnFormat(Exception exception, string format, params object[] args)
        {
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public bool IsDebugEnabled
        {
            get
            {
                return false;
            }
        }

        public bool IsErrorEnabled
        {
            get
            {
                return false;
            }
        }

        public bool IsFatalEnabled
        {
            get
            {
                return false;
            }
        }

        [Obsolete("Use IsFatalEnabled instead")]
        public bool IsFatalErrorEnabled
        {
            get
            {
                return false;
            }
        }

        public bool IsInfoEnabled
        {
            get
            {
                return false;
            }
        }

        public bool IsWarnEnabled
        {
            get
            {
                return false;
            }
        }
    }
}