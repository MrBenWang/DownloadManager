using System;
using System.Collections.Generic;
using System.Linq;

namespace DownloadManager.DownloadBase
{
    /// <summary>
    /// control downloading item list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DownloadControlerAbstract<T> where T : DownloadWorkItemAbstract
    {
        private readonly int m_MaxDownloadCount;
        private readonly object m_lockList = new object();
        private readonly Dictionary<string, T> m_DownloadResourceList;

        protected event EventHandler<DownloadStatusChangedEventArgs> OnDownloadStatusChanged;

        public string ControlerKey { get; private set; }

        public DownloadControlerAbstract(string _controlerKey, int _maxCount)
        {
            ControlerKey = _controlerKey;
            m_MaxDownloadCount = _maxCount;
            m_DownloadResourceList = new Dictionary<string, T>();
        }

        public bool IsFullDownloading
        {
            get
            {
                lock (m_lockList)
                {
                    return m_DownloadResourceList.Count >= m_MaxDownloadCount;
                }
            }
        }

        /// <summary>
        /// If add resource successed, start downloading immediately.
        /// </summary>
        /// <param name="_targetResource"></param>
        /// <returns></returns>
        public ExecuteResult AddResource(T _targetResource)
        {
            if (_targetResource.ResourceInfo == null)
            {
                return ExecuteResult.Faile;
            }

            lock (m_lockList)
            {
                if (m_DownloadResourceList.ContainsKey(_targetResource.ResourceInfo.ResourceKey))
                {
                    OnDownloadStatusChanged.Invoke(this, new DownloadStatusChangedEventArgs(_targetResource, DownloadStatus.Downloading));
                    return ExecuteResult.Success;
                }

                var minPriorityResource = GetMinPriorityInDownloadingList();
                if (IsTargetRescourceHighPriority(_targetResource.ResourceInfo, minPriorityResource.ResourceInfo))
                {
                    // target resource higher, stop the minimum priority in downloading list, change status to WaitForDownload.
                    LogInstance.Instance.LogInfo($"Stop downloading list resource that min priority[{minPriorityResource.ResourceInfo.ResourceKey}] and start downloan target resource[{_targetResource.ResourceInfo.ResourceKey}].");
                    minPriorityResource.DownloadStop();
                    OnDownloadStatusChanged.Invoke(this, new DownloadStatusChangedEventArgs(minPriorityResource, DownloadStatus.WaitForDownload));

                    m_DownloadResourceList.Add(_targetResource.ResourceInfo.ResourceKey, _targetResource);
                    _targetResource.OnDownloadingStatusChanged += OnDownloadStatusChanged;
                    _targetResource.DownloadStart();
                    OnDownloadStatusChanged.Invoke(this, new DownloadStatusChangedEventArgs(_targetResource, DownloadStatus.Downloading));
                    return ExecuteResult.Success;
                }
                else
                {
                    // the minimum priority higher, change target resource to WaitForDownload.
                    LogInstance.Instance.LogInfo($"Target resource[{_targetResource.ResourceInfo.ResourceKey}] will wait for download.");
                    OnDownloadStatusChanged.Invoke(this, new DownloadStatusChangedEventArgs(_targetResource, DownloadStatus.WaitForDownload));
                    return ExecuteResult.Success;
                }
            }
        }

        /// <summary>
        /// 1、DownloadImmediatelyPriority: User operate immediately downloading, 0 is the min;
        /// 2、DownloadPriority: Preset priority level, 0 is the max.
        /// </summary>
        /// <returns></returns>
        private T GetMinPriorityInDownloadingList()
        {
            T _rom = m_DownloadResourceList.Values.OrderBy(m => m.ResourceInfo.DownloadImmediatelyPriority).OrderByDescending(m => m.ResourceInfo.DownloadPriority).FirstOrDefault();

            LogInstance.Instance.LogInfo($"Current downloading count is max value, will get min download priority resource from the downloading list, resourceID:[{_rom.ResourceInfo.ResourceID}], DownloadUrl:[{_rom.ResourceInfo.DownloadUrl}], downloadPriority:[{_rom.ResourceInfo.DownloadPriority}], immediatelyPriority:[{_rom.ResourceInfo.DownloadImmediatelyPriority}].");
            return _rom;
        }

        /// <summary>
        /// compare targetResource and minPriorityDownloading priority
        /// </summary>
        /// <param name="_targetResource">prepare add resource</param>
        /// <param name="_minPriorityDownloading">min priority in downloading list</param>
        /// <returns></returns>
        private bool IsTargetRescourceHighPriority(DownloadResourceAbstract _targetResource, DownloadResourceAbstract _minPriorityDownloading)
        {
            if (_targetResource.DownloadImmediatelyPriority > _minPriorityDownloading.DownloadImmediatelyPriority)
            {
                return true;
            }
            else if (_targetResource.DownloadImmediatelyPriority < _minPriorityDownloading.DownloadImmediatelyPriority)
            {
                return false;
            }
            // the DownloadImmediatelyPriority is the same value
            else if (_targetResource.DownloadPriority < _minPriorityDownloading.DownloadPriority)
            {
                return true;
            }
            else if (_targetResource.DownloadPriority > _minPriorityDownloading.DownloadPriority)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Pause is stop
        /// </summary>
        /// <param name="_targetResource"></param>
        /// <returns></returns>
        public ExecuteResult DownloadPause(T _targetResource)
        {
            if (_targetResource.ResourceInfo == null)
            {
                return ExecuteResult.Faile;
            }

            lock (m_lockList)
            {
                string _resourceKey = _targetResource.ResourceInfo.ResourceKey;
                if (m_DownloadResourceList.ContainsKey(_resourceKey))
                {
                    var _item = m_DownloadResourceList[_resourceKey];
                    if (_item != null)
                    {
                        _item.DownloadStop();
                        OnDownloadStatusChanged.Invoke(this, new DownloadStatusChangedEventArgs(_item, DownloadStatus.DownloadPause));
                        m_DownloadResourceList.Remove(_resourceKey);
                        return ExecuteResult.Success;
                    }
                }

                return ExecuteResult.Faile;
            }
        }

        public ExecuteResult RemoveResource(T _targetResource)
        {
            lock (m_lockList)
            {
                if (m_DownloadResourceList.ContainsKey(_targetResource.ResourceInfo.ResourceKey))
                {
                    m_DownloadResourceList.Remove(_targetResource.ResourceInfo.ResourceKey);
                }

                return ExecuteResult.Success;
            }
        }

        public ExecuteResult DownloadStopAllResources()
        {
            lock (m_lockList)
            {
                foreach (var _item in m_DownloadResourceList.Values)
                {
                    _item.DownloadStop();
                }

                m_DownloadResourceList.Clear();
                return ExecuteResult.Success;
            }
        }
    }
}