using DownloadManager.DownloadBase;
using DownloadManager.DownloadBase.HTTP;
using System;
using System.Timers;

namespace DownloadManager.MyDownloadA
{
    /// <summary>
    /// when application start-up, automatically download. Use HTTP protocol.
    /// </summary>
    public sealed class AutoHttpDownloadDealer : IDownloadTask<AutoResource>
    {
        public string ControlName => "AutoDownload";

        private const int MAX_COUNT = 5;
        private readonly string _key;
        private LocalDataStorage m_LocalDataStorage;
        private HttpDownloadControler m_controler;
        private static AutoHttpDownloadDealer _instance;
        private Timer m_timer = null;

        public static AutoHttpDownloadDealer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AutoHttpDownloadDealer();
                }

                return _instance;
            }
        }

        private AutoHttpDownloadDealer()
        {
            _key = Guid.NewGuid().ToString();
            m_LocalDataStorage = LocalDataStorage.Current;
            m_controler = new HttpDownloadControler(_key, MAX_COUNT, ControlName);
            m_controler.GetNextWairForDownloadResource += () =>
            {
                return m_LocalDataStorage.GetNextWairForDownloadAutoResource();
            };
            m_controler.OnUpdateStatusCallBack += (obj, e) =>
            {
                m_LocalDataStorage.UpdateAutoResourceStatus(e.DownloadResource.ResourceInfo.DatabaseID, e.DownloadStatus);
            };

            ContinueDownloading();
        }

        public void Start()
        {
            m_timer = new Timer();
            m_timer.Elapsed += new ElapsedEventHandler(DownloadFailedToReStart);
            m_timer.Interval = 60 * 1000;
            m_timer.Start();
        }

        public void Stop()
        {
            DownloadStopAll();
            m_timer.Stop();
            m_timer.Dispose();
            m_timer = null;
        }

        /// <summary>
        /// continue downloading, if not full downloading, then add resource.
        /// </summary>
        private void ContinueDownloading()
        {
            LogInstance.Instance.LogInfo($"Begin {ControlName} Continue Downloading.");

            var downloadingRomList = m_LocalDataStorage.GetAllDownloadingAutoResources();
            foreach (var item in downloadingRomList)
            {
                DownloadStart(item);
            }

            DownloadFailedToReStart(null, null);
            LogInstance.Instance.LogInfo($"End {ControlName}.Continue Downloading Success.");
        }

        private void DownloadFailedToReStart(object sender, ElapsedEventArgs e)
        {
            LogInstance.Instance.LogInfo($"{ControlName}.DownloadFailedToReStart Start.");
            m_LocalDataStorage.ResetNoSuccessFlashToolStatus();
            m_controler.LoopPushResourceUntilDownloadMax();
        }

        public ExecuteResult DownloadStart(AutoResource _res)
        {
            LogInstance.Instance.LogInfo($"{ControlName}.DownloadStart ResourceID:[{_res.ResourceID}], DownloadUrl:[{_res.DownloadUrl}], downloadPriority:[{_res.DownloadPriority}], immediatelyPriority:[{_res.DownloadImmediatelyPriority}].");

            m_controler.AddResource(new HttpDownloadWorkItem(_res));
            return ExecuteResult.Success;
        }

        public ExecuteResult DownloadStart(long _resourceID, string _url)
        {
            try
            {
                var _res = m_LocalDataStorage.GetAutoResourceByResourceID(_resourceID, _url);
                if (_res != null)
                {
                    return DownloadStart(_res);
                }
                else
                {
                    LogInstance.Instance.LogInfo($"{ControlName}.DownloadStart ResourceID:[{_resourceID}], DownloadUrl:[{_res.DownloadUrl}] from database, result is null.");
                    return ExecuteResult.Faile;
                }
            }
            catch (Exception ex)
            {
                LogInstance.Instance.LogError($"{ControlName}.DownloadStart ResourceID:[{_resourceID}], DownloadUrl:{_url}] Exception:{ex}.");
                return ExecuteResult.Exception;
            }
        }

        public ExecuteResult DownloadStopAll()
        {
            try
            {
                LogInstance.Instance.LogInfo($"Begin {ControlName}.DownloadStopAll.");
                m_controler.DownloadStopAllResources();
                LogInstance.Instance.LogInfo($"End {ControlName}.DownloadStopAll Success.");
                return ExecuteResult.Success;
            }
            catch (Exception ex)
            {
                LogInstance.Instance.LogError($"{ControlName}.DownloadStopAll Exception:{ex}.");
                return ExecuteResult.Exception;
            }
        }

        #region Don't need those function

        public ExecuteResult DownloadImmediately(AutoResource _res)
        {
            throw new NotImplementedException();
        }

        public ExecuteResult DownloadImmediately(long _resourceID, string _url)
        {
            throw new NotImplementedException();
        }

        public ExecuteResult DownloadPause(AutoResource _res)
        {
            throw new NotImplementedException();
        }

        public ExecuteResult DownloadPause(long _resourceID, string _url)
        {
            throw new NotImplementedException();
        }

        #endregion Don't need those function
    }
}