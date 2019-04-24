using System;
using Microsoft.Extensions.Logging;
using NLog;
using Perfusion;

namespace JetKarmaBot
{
    public class NLoggerFactory : ILoggerFactory
    {
        [Inject]
        private NLoggerProvider c;
        public void AddProvider(ILoggerProvider provider)
        {
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) => c.CreateLogger(categoryName);

        public void Dispose()
        {
        }
    }
    public class NLoggerProvider : ILoggerProvider
    {
        [Inject]
        private Container c;

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) => new LoggerVirtualizer(LogManager.GetLogger(categoryName));

        public void Dispose()
        {
        }
    }

    public class LoggerVirtualizer : Microsoft.Extensions.Logging.ILogger
    {
        private Logger logger;

        public LoggerVirtualizer(Logger logger)
        {
            this.logger = logger;
        }

        private Microsoft.Extensions.Logging.LogLevel getAppropriate(NLog.LogLevel level)
        {
            if (level == NLog.LogLevel.Trace)
                return Microsoft.Extensions.Logging.LogLevel.Trace;
            else if (level == NLog.LogLevel.Debug)
                return Microsoft.Extensions.Logging.LogLevel.Debug;
            else if (level == NLog.LogLevel.Info)
                return Microsoft.Extensions.Logging.LogLevel.Information;
            else if (level == NLog.LogLevel.Warn)
                return Microsoft.Extensions.Logging.LogLevel.Warning;
            else if (level == NLog.LogLevel.Error)
                return Microsoft.Extensions.Logging.LogLevel.Error;
            else if (level == NLog.LogLevel.Fatal)
                return Microsoft.Extensions.Logging.LogLevel.Critical;
            else if (level == NLog.LogLevel.Off)
                return Microsoft.Extensions.Logging.LogLevel.None;
            else
                return Microsoft.Extensions.Logging.LogLevel.None;
        }
        private NLog.LogLevel getAppropriate(Microsoft.Extensions.Logging.LogLevel level)
        {
            switch (level)
            {
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    return NLog.LogLevel.Trace;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    return NLog.LogLevel.Info;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    return NLog.LogLevel.Warn;
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    return NLog.LogLevel.Error;
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    return NLog.LogLevel.Fatal;
                default:
                    return NLog.LogLevel.Off;
            }
        }

        public IDisposable BeginScope<TState>(TState state) => new SomeDisposable();

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return logger.IsEnabled(getAppropriate(logLevel));
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = state.ToString();
            if (exception != null) logger.Log(getAppropriate(logLevel), exception, formatter(state, exception));
            else logger.Log(getAppropriate(logLevel), state);
        }
    }
    public class SomeDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}