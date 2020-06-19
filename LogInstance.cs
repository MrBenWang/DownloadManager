using log4net;

namespace DownloadManager
{
    internal class LogInstance
    {
        private static LogInstance _instance;

        public static LogInstance Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogInstance();
                }

                return _instance;
            }
        }

        private LogInstance()
        {
        }

        private ILog logger;

        public void LogInfo(string info)
        {
            logger.Info(info);
        }

        public void LogWarn(string info)
        {
            logger.Warn(info);
        }

        public void LogError(string info)
        {
            logger.Error(info);
        }

        public void LogDebug(string info)
        {
            logger.Debug(info);
        }
    }
}