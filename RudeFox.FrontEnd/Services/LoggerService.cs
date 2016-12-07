using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
namespace RudeFox.Services
{
    public sealed class LoggerService
    {
        #region Constructor
        private LoggerService()
        {

        }
        #endregion

        #region Fields
        private static Logger _logger = LogManager.GetLogger("RudeFox.Services.LoggerService");
        #endregion

        #region Properties
        private static readonly Lazy<LoggerService> _instance = new Lazy<LoggerService>(() => new LoggerService());
        public static LoggerService Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        #region Methods
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
        #endregion
    }
}
