using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Targets;
using NLog.Config;
using SharpRaven;
using NLog;
using SharpRaven.Data;
using NLog.Common;
using System.Diagnostics;

namespace Permadelete.Nlog
{
    [Target("Sentry")]
    public sealed class SentryTarget : TargetWithLayout
    {
        #region Constructor
        public SentryTarget()
        {
#if CLASSIC
            if (string.IsNullOrWhiteSpace(Keys.SENTRY_API_DSN))
                throw new NullReferenceException("Bad Sentry API DSN.");

            _ravenClient = new RavenClient(Keys.SENTRY_API_DSN);
            _ravenClient.Release = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
#endif
        }
        #endregion

        #region Fields
        private RavenClient _ravenClient;
        #endregion

        #region Methods
        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent.Exception == null)
                SendRaven(logEvent.FormattedMessage, logEvent.Level);
            else
                SendRaven(logEvent.Exception);
        }

        [Conditional("CLASSIC")]
        private void SendRaven(string message, LogLevel level)
        {
            var sentryEvent = new SentryEvent(message);
            sentryEvent.Level = ParseLogLevel(level);
            _ravenClient.Capture(sentryEvent);
        }

        [Conditional("CLASSIC")]
        private void SendRaven(Exception ex)
        {
            var sentryEvent = new SentryEvent(ex);
            sentryEvent.Level = ErrorLevel.Error;
            _ravenClient.Capture(sentryEvent);
        }

        private ErrorLevel ParseLogLevel(LogLevel level)
        {
            switch (level.Ordinal)
            {
                case 0: // Trace
                    return ErrorLevel.Debug;
                case 1: // Debug
                    return ErrorLevel.Debug;
                case 2: // Info
                    return ErrorLevel.Info;
                case 3: // Warn
                    return ErrorLevel.Warning;
                case 4: // Error
                    return ErrorLevel.Error;
                case 5: // Fatal
                    return ErrorLevel.Fatal;
                default:
                    return ErrorLevel.Info;
            }
        }
        #endregion
    }
}
