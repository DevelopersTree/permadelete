using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using SharpRaven.Data;
using NLog.Targets;

namespace Permadelete.Services
{
    public sealed class LoggerService
    {
        #region Constructor
        static LoggerService()
        {
#if WINDOWS_STORE
            _logger = LogManager.CreateNullLogger();
#else
            Target.Register<Nlog.SentryTarget>("Sentry");
            _logger = LogManager.GetLogger("Permadelete.Services.LoggerService");
#endif

#if CLASSIC
            _ravenClient = new SharpRaven.RavenClient(Keys.SENTRY_API_DSN);
            _ravenClient.Release = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
#endif
        }
        private LoggerService()
        {
            
        }
        #endregion

        #region Fields
        private static Logger _logger;
        private static SharpRaven.RavenClient _ravenClient;
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

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Error(Exception ex)
        {
            _logger.Error(ex);
        }

        public async Task SendRaven(string message) => await SendRaven(message, ErrorLevel.Info);
        public async Task SendRaven(string message, ErrorLevel level)
        {
            if (_ravenClient == null) return;

            var sentryEvent = new SentryEvent(message);
            sentryEvent.Level = level;
            await _ravenClient.CaptureAsync(sentryEvent);
        }
        #endregion
    }
}
