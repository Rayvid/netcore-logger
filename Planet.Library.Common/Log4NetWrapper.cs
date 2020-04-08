using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Planet.Library.Common;

namespace Planet.Library.Logging
{
    // Loading configuration is on you, this wrapper assumes log4net and DI is configured
    public class Log4NetWrapper : log4net.ILog, log4net.Core.ILogger
    {
        private log4net.ILog _logger;
        private static AsyncLocal<ILogger> _scopedLogger = new AsyncLocal<ILogger>();
        private Func<bool> _createNewIfOutOfScope = () => false;

        public Log4NetWrapper(Type category)
        {
            _createNewIfOutOfScope = () => {
                if (_scopedLogger.Value == null)
                {
                    _scopedLogger.Value = ((ILoggerFactory)PlanetLibs.ServiceProvider.GetService(typeof(ILoggerFactory))).CreateLogger(category);
                }
                return true;
            };
            _logger = log4net.LogManager.GetLogger(category);
        }

        public Log4NetWrapper(string category)
        {
            _createNewIfOutOfScope = () => {
                if (_scopedLogger.Value == null)
                {
                    _scopedLogger.Value = ((ILoggerFactory)PlanetLibs.ServiceProvider.GetService(typeof(ILoggerFactory))).CreateLogger(category);
                }
                return true;
            };
            _logger = log4net.LogManager.GetLogger(category, category);
        }

        public static log4net.ILog GetLogger(Type category)
        {
            return new Log4NetWrapper(category);
        }

        public static log4net.ILog GetLogger(string category)
        {
            return new Log4NetWrapper(category);
        }

        public bool IsDebugEnabled => _logger.IsDebugEnabled;

        public bool IsInfoEnabled => _logger.IsInfoEnabled;

        public bool IsWarnEnabled => _logger.IsWarnEnabled;

        public bool IsErrorEnabled => _logger.IsErrorEnabled;

        public bool IsFatalEnabled => _logger.IsFatalEnabled;

        public log4net.Core.ILogger Logger => this;

        public string Name => _logger.Logger.Name;

        public log4net.Repository.ILoggerRepository Repository => _logger.Logger.Repository;

        public void Debug(object message)
        {
            _logger.Debug(message);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogDebug(message.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            _logger.Debug(message);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogDebug(exception, message.ToString());
        }

        public void DebugFormat(string format, params object[] args)
        {
            _logger.DebugFormat(format, args);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogDebug(format, args);
        }

        public void DebugFormat(string format, object arg0)
        {
            _logger.DebugFormat(format, arg0);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogDebug(format, arg0);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            _logger.DebugFormat(format, arg0, arg1);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogDebug(format, new[] { arg0, arg1 });
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            _logger.DebugFormat(format, arg0, arg1, arg2);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogDebug(format, new[] { arg0, arg1, arg2 });
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            _logger.DebugFormat(provider, format, args);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogDebug(format, args);
        }

        public void Error(object message)
        {
            _logger.Error(message);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogError(message.ToString());
        }

        public void Error(object message, Exception exception)
        {
            _logger.Error(message);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogError(exception, message.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            _logger.ErrorFormat(format, args);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogError(format, args);
        }

        public void ErrorFormat(string format, object arg0)
        {
            _logger.ErrorFormat(format, arg0);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogError(format, arg0);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            _logger.ErrorFormat(format, arg0, arg1);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogError(format, new[] { arg0, arg1 });
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            _logger.ErrorFormat(format, arg0, arg1, arg2);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogError(format, new[] { arg0, arg1, arg2 });
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            _logger.ErrorFormat(provider, format, args);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogError(format, args);
        }

        public void Fatal(object message)
        {
            _logger.Fatal(message);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogCritical(message.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
            _logger.Fatal(message);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogCritical(exception, message.ToString());
        }

        public void FatalFormat(string format, params object[] args)
        {
            _logger.FatalFormat(format, args);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogCritical(format, args);
        }

        public void FatalFormat(string format, object arg0)
        {
            _logger.FatalFormat(format, arg0);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogCritical(format, arg0);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            _logger.FatalFormat(format, arg0, arg1);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogCritical(format, new[] { arg0, arg1 });
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            _logger.FatalFormat(format, arg0, arg1, arg2);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogCritical(format, new[] { arg0, arg1, arg2 });
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            _logger.FatalFormat(provider, format, args);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogCritical(format, args);
        }

        public void Info(object message)
        {
            _logger.Info(message);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogInformation(message.ToString());
        }

        public void Info(object message, Exception exception)
        {
            _logger.Info(message);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogInformation(exception, message.ToString());
        }

        public void InfoFormat(string format, params object[] args)
        {
            _logger.InfoFormat(format, args);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogInformation(format, args);
        }

        public void InfoFormat(string format, object arg0)
        {
            _logger.InfoFormat(format, arg0);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogInformation(format, arg0);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            _logger.InfoFormat(format, arg0, arg1);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogInformation(format, new[] { arg0, arg1 });
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            _logger.InfoFormat(format, arg0, arg1, arg2);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogInformation(format, new[] { arg0, arg1, arg2 });
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            _logger.InfoFormat(provider, format, args);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogInformation(format, args);
        }

        public void Warn(object message)
        {
            _logger.Warn(message);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogWarning(message.ToString());
        }

        public void Warn(object message, Exception exception)
        {
            _logger.Warn(message);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogWarning(exception, message.ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            _logger.WarnFormat(format, args);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogWarning(format, args);
        }

        public void WarnFormat(string format, object arg0)
        {
            _logger.WarnFormat(format, arg0);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogWarning(format, arg0);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            _logger.WarnFormat(format, arg0, arg1);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogWarning(format, new[] { arg0, arg1 });
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            _logger.WarnFormat(format, arg0, arg1, arg2);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogWarning(format, new[] { arg0, arg1, arg2 });
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            _logger.WarnFormat(provider, format, args);
            if (_createNewIfOutOfScope()) _scopedLogger.Value.LogWarning(format, args);
        }

        public void Log(Type callerStackBoundaryDeclaringType, log4net.Core.Level level, object message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Log(log4net.Core.LoggingEvent logEvent)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabledFor(log4net.Core.Level level)
        {
            return _logger.Logger.IsEnabledFor(level);
        }
    }
}
