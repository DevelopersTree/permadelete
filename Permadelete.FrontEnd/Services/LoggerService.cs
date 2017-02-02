using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using SharpRaven.Data;

namespace Permadelete.Services
{
    public sealed class LoggerService
    {
        #region Constructor
        private LoggerService()
        {
            _ravenClient.Release = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        #endregion

        #region Fields
        private static Logger _logger = LogManager.GetLogger("Permadelete.Services.LoggerService");
        private static SharpRaven.RavenClient _ravenClient = new SharpRaven.RavenClient(Keys.SENTRY_API_DSN);
        #endregion

        #region Properties
        private static readonly Lazy<LoggerService> _instance = new Lazy<LoggerService>(() => new LoggerService());
        public static LoggerService Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        #region Methods
        public void Warning(Exception ex)
        {
            _logger.Warn(ex);
        }

        public void Info(string message) => Info(() => message);
        public void Info(Func<string> messageFunction)
        {
            _logger.Info(messageFunction);
        }

        public void Error(string message) => Error(() => message);
        public void Error(Func<string> messageFunction)
        {
            _logger.Error(messageFunction);
        }
        public void Error(Exception ex)
        {
            _logger.Error(ex);
        }

        public void Fatal(string message) => Fatal(() => message);
        public void Fatal(Func<string> messageFunction)
        {
            _logger.Fatal(messageFunction);
        }

        public async Task SendRaven(string message) => await SendRaven(message, ErrorLevel.Info);
        public async Task SendRaven(string message, ErrorLevel level)
        {
            var sentryEvent = new SentryEvent(message);
            sentryEvent.Level = level;
            await _ravenClient.CaptureAsync(sentryEvent);
        }
        #endregion
    }
}
