using NLog;

namespace WebApplicationUnipi.Services.Factories
{
    public class MyLogger : Interfaces.ILogger
    {
        //singleton pattern example. Only one instance of this class can be instanciated
        private static MyLogger instance; //singleton design instance of this class
        private static Logger logger; //static variable to hold a single instance of the nlog
        private string theLogger = "WebApplicationUnipiLoggerRules";

        public MyLogger()
        {
        }

        public static MyLogger getInstance()
        {
            if (instance == null)
                instance = new MyLogger();
            return instance;
        }

        private Logger GetLogger(string theLogger)
        {
            if (logger == null)
                logger = LogManager.GetLogger(theLogger);

            return logger;
        }

        public void Debug(string message, string arg = null)
        {
            if (arg == null)
                GetLogger(theLogger).Debug(message);
            else
                GetLogger(theLogger).Debug(message, arg);
        }

        public void Error(string message, string arg = null)
        {
            if (arg == null)
                GetLogger(theLogger).Error(message);
            else
                GetLogger(theLogger).Error(message, arg);
        }

        public void Info(string message, string arg = null)
        {
            if (arg == null)
                GetLogger(theLogger).Info(message);
            else
                GetLogger(theLogger).Info(message, arg);
        }

        public void Warning(string message, string arg = null)
        {
            if (arg == null)
                GetLogger(theLogger).Warn(message);
            else
                GetLogger(theLogger).Warn(message, arg);
        }
    }
}