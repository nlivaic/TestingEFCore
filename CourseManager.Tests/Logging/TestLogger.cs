using System;
using Microsoft.Extensions.Logging;

namespace CourseManager.Tests.Logging
{
    public class TestLogger : ILogger
    {
        private readonly Action<string> _logTo;
        private readonly LogLevel _level;

        public TestLogger(Action<string> logTo, LogLevel level)
        {
            _logTo = logTo;
            _level = level;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _level;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logTo(state.ToString());
        }
    }
}