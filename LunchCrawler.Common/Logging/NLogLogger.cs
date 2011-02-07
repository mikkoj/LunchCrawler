using System;

using NLog;


namespace LunchCrawler.Common.Logging
{
    public class NLogLogger : ILogger
    {
        internal NLogLogger() {}


        public NLogLogger(Logger logger, NLogFactory factory)
        {
            Logger = logger;
            Factory = factory;
        }


        protected internal NLogFactory Factory { get; set; }
        protected internal Logger Logger { get; set; }

        #region ILogger Members

        public virtual ILogger CreateChildLogger(string name)
        {
            return Factory.Create(Logger.Name + "." + name);
        }


        public void Debug(string message)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(message);
            }
        }


        [Obsolete("Use DebugFormat instead")]
        public void Debug(string format, params object[] args)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(format, args);
            }
        }


        public void Debug(string message, Exception exception)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugException(message, exception);
            }
        }


        public void DebugFormat(string format, params object[] args)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(format, args);
            }
        }


        public void DebugFormat(Exception exception, string format, params object[] args)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugException(string.Format(format, args), exception);
            }
        }


        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(formatProvider, format, args);
            }
        }


        public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugException(string.Format(formatProvider, format, args), exception);
            }
        }


        public void Error(string message)
        {
            if (Logger.IsErrorEnabled)
            {
                Logger.Error(message);
            }
        }


        public void Error(string message, Exception exception)
        {
            if (Logger.IsErrorEnabled)
            {
                Logger.ErrorException(message, exception);
            }
        }


        [Obsolete("Use ErrorFormat instead")]
        public void Error(string format, params object[] args)
        {
            if (Logger.IsErrorEnabled)
            {
                Logger.Error(format, args);
            }
        }


        public void ErrorFormat(string format, params object[] args)
        {
            if (Logger.IsErrorEnabled)
            {
                Logger.Error(format, args);
            }
        }


        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
            if (Logger.IsErrorEnabled)
            {
                Logger.ErrorException(string.Format(format, args), exception);
            }
        }


        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (Logger.IsErrorEnabled)
            {
                Logger.Error(formatProvider, format, args);
            }
        }


        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (Logger.IsErrorEnabled)
            {
                Logger.ErrorException(string.Format(formatProvider, format, args), exception);
            }
        }


        public void Fatal(string message)
        {
            if (Logger.IsFatalEnabled)
            {
                Logger.Fatal(message);
            }
        }


        public void Fatal(string message, Exception exception)
        {
            if (Logger.IsFatalEnabled)
            {
                Logger.FatalException(message, exception);
            }
        }


        [Obsolete("Use FatalFormat instead")]
        public void Fatal(string format, params object[] args)
        {
            if (Logger.IsFatalEnabled)
            {
                Logger.Fatal(format, args);
            }
        }


        [Obsolete("Use Fatal instead")]
        public void FatalError(string message)
        {
            if (Logger.IsFatalEnabled)
            {
                Logger.Fatal(message);
            }
        }


        [Obsolete("Use Fatal instead")]
        public void FatalError(string message, Exception exception)
        {
            if (Logger.IsFatalEnabled)
            {
                Logger.FatalException(message, exception);
            }
        }


        [Obsolete("Use FatalFormat instead")]
        public void FatalError(string format, params object[] args)
        {
            if (Logger.IsFatalEnabled)
            {
                Logger.Fatal(format, args);
            }
        }


        public void FatalFormat(string format, params object[] args)
        {
            if (Logger.IsFatalEnabled)
            {
                Logger.Fatal(format, args);
            }
        }


        public void FatalFormat(Exception exception, string format, params object[] args)
        {
            if (Logger.IsFatalEnabled)
            {
                Logger.FatalException(string.Format(format, args), exception);
            }
        }


        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (Logger.IsFatalEnabled)
            {
                Logger.Fatal(formatProvider, format, args);
            }
        }


        public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (Logger.IsFatalEnabled)
            {
                Logger.FatalException(string.Format(formatProvider, format, args), exception);
            }
        }


        public void Info(string message)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info(message);
            }
        }


        [Obsolete("Use InfoFormat instead")]
        public void Info(string format, params object[] args)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info(format, args);
            }
        }


        public void Info(string message, Exception exception)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.InfoException(message, exception);
            }
        }


        public void InfoFormat(string format, params object[] args)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info(format, args);
            }
        }


        public void InfoFormat(Exception exception, string format, params object[] args)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.InfoException(string.Format(format, args), exception);
            }
        }


        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info(formatProvider, format, args);
            }
        }


        public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.InfoException(string.Format(formatProvider, format, args), exception);
            }
        }


        public void Warn(string message)
        {
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn(message);
            }
        }


        public void Warn(string message, Exception exception)
        {
            if (Logger.IsWarnEnabled)
            {
                Logger.WarnException(message, exception);
            }
        }


        [Obsolete("Use WarnFormat instead")]
        public void Warn(string format, params object[] args)
        {
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn(format, args);
            }
        }


        public void WarnFormat(string format, params object[] args)
        {
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn(format, args);
            }
        }


        public void WarnFormat(Exception exception, string format, params object[] args)
        {
            if (Logger.IsWarnEnabled)
            {
                Logger.WarnException(string.Format(format, args), exception);
            }
        }


        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (Logger.IsWarnEnabled)
            {
                Logger.Warn(formatProvider, format, args);
            }
        }


        public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (Logger.IsWarnEnabled)
            {
                Logger.WarnException(string.Format(formatProvider, format, args), exception);
            }
        }


        public bool IsDebugEnabled
        {
            get { return Logger.IsDebugEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return Logger.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return Logger.IsFatalEnabled; }
        }

        [Obsolete("Use IsFatalEnabled instead")]
        public bool IsFatalErrorEnabled
        {
            get { return Logger.IsFatalEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return Logger.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return Logger.IsWarnEnabled; }
        }

        #endregion

        public override string ToString()
        {
            return Logger.ToString();
        }
    }
}