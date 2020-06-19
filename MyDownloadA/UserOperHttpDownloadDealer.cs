using DownloadManager.DownloadBase;
using DownloadManager.DownloadBase.HTTP;
using System;

namespace DownloadManager.MyDownloadA
{
    /// <summary>
    /// user operate download resource, e.g.  DownloadImmediately, DownloadPause
    /// </summary>
    public sealed class UserOperHttpDownloadDealer : IDownloadTask<UserOperRescource>
    {
        public string ControlName => "UserOperate";

        private const int MAX_COUNT = 5;
        private readonly string _key;
        private LocalDataStorage m_LocalDataStorage;
        private HttpDownloadControler m_controler;
        private static UserOperHttpDownloadDealer _instance;

        public static UserOperHttpDownloadDealer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserOperHttpDownloadDealer();
                }

                return _instance;
            }
        }

        private UserOperHttpDownloadDealer()
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
        }

        public ExecuteResult DownloadImmediately(UserOperRescource _res)
        {
            LogInstance.Instance.LogInfo($"{ControlName}.DownloadImmediately ResourceID:[{_res.ResourceID}], DownloadUrl:[{_res.DownloadUrl}], downloadPriority:[{_res.DownloadPriority}], immediatelyPriority:[{_res.DownloadImmediatelyPriority}].");

            m_controler.AddResource(new HttpDownloadWorkItem(_res));
            return ExecuteResult.Success;
        }

        public ExecuteResult DownloadImmediately(long _resourceID, string _url)
        {
            try
            {
                var _res = m_LocalDataStorage.GetUserOperResourceSetHighestPriority(_resourceID, _url);
                if (_res != null)
                {
                    return DownloadImmediately(_res);
                }
                else
                {
                    LogInstance.Instance.LogWarn($"{ControlName}.DownloadImmediately ResourceID:[{_resourceID}], DownloadUrl:{_url}] from database, result is null.");
                    return ExecuteResult.Faile;
                }
            }
            catch (Exception ex)
            {
                LogInstance.Instance.LogError($"{ControlName}.DownloadImmediately ResourceID:[{_resourceID}], DownloadUrl:{_url}] Exception:{ex}.");
                return ExecuteResult.Exception;
            }
        }

        public ExecuteResult DownloadPause(UserOperRescource _res)
        {
            LogInstance.Instance.LogInfo($"{ControlName}.DownloadPause ResourceID:[{_res.ResourceID}], DownloadUrl:[{_res.DownloadUrl}].");
            return m_controler.DownloadPause(new HttpDownloadWorkItem(_res));
        }

        public ExecuteResult DownloadPause(long _resourceID, string _url)
        {
            try
            {
                var _res = m_LocalDataStorage.GetUserOperResourceByResourceID(_resourceID, _url);
                if (_res != null)
                {
                    return DownloadPause(_res);
                }
                else
                {
                    LogInstance.Instance.LogInfo($"{ControlName}.DownloadPause ResourceID:[{_resourceID}], DownloadUrl:{_url}] from database, result is null.");
                    return ExecuteResult.Faile;
                }
            }
            catch (Exception ex)
            {
                LogInstance.Instance.LogError($"{ControlName}.DownloadPause ResourceID:[{_resourceID}], DownloadUrl:{_url}] Exception:{ex}.");
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

        public ExecuteResult DownloadStart(UserOperRescource _res)
        {
            throw new NotImplementedException();
        }

        public ExecuteResult DownloadStart(long _resourceID, string _url)
        {
            throw new NotImplementedException();
        }

        #endregion Don't need those function
    }
}