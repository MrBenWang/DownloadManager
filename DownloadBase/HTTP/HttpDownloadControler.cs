using System;

namespace DownloadManager.DownloadBase.HTTP
{
    public class HttpDownloadControler : DownloadControlerAbstract<HttpDownloadWorkItem>
    {
        public HttpDownloadControler(string _controlerKey, int _maxCount, string _controlerName)
                : base(_controlerKey, _maxCount)
        {
            ControlerName = _controlerName;
            OnDownloadStatusChanged += m_OnDownloadStatusChanged;
        }

        public string ControlerName { get; private set; }

        public delegate DownloadResourceAbstract NextDownloadResource();

        public event NextDownloadResource GetNextWairForDownloadResource;

        public event EventHandler<DownloadStatusChangedEventArgs> OnUpdateStatusCallBack;

        /// <summary>
        /// add resource to downloading list, until downloading.count equal MAX_COUNT
        /// </summary>
        public void LoopPushResourceUntilDownloadMax()
        {
            DownloadResourceAbstract _nexResource;
            while (!IsFullDownloading)
            {
                _nexResource = GetNextWairForDownloadResource();
                if (_nexResource == null)
                {
                    break; // no wait for download resource
                }

                AddResource(new HttpDownloadWorkItem(_nexResource));
            }
        }

        private void FireBeginNextDownloadResource(DownloadResourceAbstract _resource)
        {
            RemoveResource(new HttpDownloadWorkItem(_resource));
            LoopPushResourceUntilDownloadMax();
        }

        private void m_OnDownloadStatusChanged(object sender, DownloadStatusChangedEventArgs e)
        {
            var _resource = e.DownloadResource.ResourceInfo;
            if (_resource == null)
            {
                return;
            }

            switch (e.DownloadStatus)
            {
                case DownloadStatus.NotStart:
                case DownloadStatus.WaitForDownload:
                    break;

                case DownloadStatus.Downloading:
                    OnUpdateStatusCallBack.Invoke(this, e);
                    break;

                case DownloadStatus.DownloadPause:
                    LogInstance.Instance.LogInfo($"{ControlerName} ResourceID:[{_resource.ResourceID}], LocalFileName:[{_resource.LocalFileName}] download pause.");

                    OnUpdateStatusCallBack.Invoke(this, e);
                    FireBeginNextDownloadResource(_resource);
                    break;

                case DownloadStatus.DownloadFailed_FileNotExist:
                    LogInstance.Instance.LogInfo($"{ControlerName} ResourceID:[{_resource.ResourceID}], LocalFileName:[{_resource.LocalFileName}] download File Not Exist.");
                    OnUpdateStatusCallBack.Invoke(this, e);
                    FireBeginNextDownloadResource(_resource);
                    break;

                case DownloadStatus.DownloadSuccess:
                    LogInstance.Instance.LogInfo($"{ControlerName} ResourceID:[{_resource.ResourceID}], LocalFileName:[{_resource.LocalFileName}] download finished, begin check md5.");
                    OnUpdateStatusCallBack.Invoke(this, new DownloadStatusChangedEventArgs(e.DownloadResource, DownloadStatus.MD5Checking));

                    // md5 check maybe spend long time
                    if (e.DownloadResource.CheckMD5())
                    {
                        LogInstance.Instance.LogInfo($"{ControlerName} ResourceID:[{_resource.ResourceID}], LocalFileName:[{_resource.LocalFileName}] md5 check success.");
                        OnUpdateStatusCallBack.Invoke(this, new DownloadStatusChangedEventArgs(e.DownloadResource, DownloadStatus.DownloadSuccess));
                    }
                    else
                    {
                        LogInstance.Instance.LogInfo($"{ControlerName} ResourceID:[{_resource.ResourceID}], LocalFileName:[{_resource.LocalFileName}] md5 check failed.");
                        OnUpdateStatusCallBack.Invoke(this, new DownloadStatusChangedEventArgs(e.DownloadResource, DownloadStatus.DownloadFailed_MD5CheckFailed));
                    }

                    FireBeginNextDownloadResource(_resource);
                    break;

                case DownloadStatus.DownloadFailed:
                    LogInstance.Instance.LogInfo($"{ControlerName} ResourceID:{_resource.ResourceID}, LocalFileName:{_resource.LocalFileName} download failed.");
                    OnUpdateStatusCallBack.Invoke(this, e);

                    FireBeginNextDownloadResource(_resource);
                    break;

                default:
                    break;
            }
        }
    }
}