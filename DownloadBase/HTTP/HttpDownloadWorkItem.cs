namespace DownloadManager.DownloadBase.HTTP
{
    public sealed class HttpDownloadWorkItem : DownloadWorkItemAbstract
    {
        private HttpClient m_httpClient = null;

        public HttpDownloadWorkItem(DownloadResourceAbstract downloadResource) : base(downloadResource)
        {
            m_httpClient = new HttpClient();
        }

        /// <summary>
        /// 改变 资源在本地库的状态，执行下载
        /// </summary>
        protected override void DoStart()
        {
            var downloadResult = DownloadResult.NetConnectError;
            string _downloadUrl = ResourceInfo.DownloadUrl;
            if (!string.IsNullOrWhiteSpace(_downloadUrl))
            {
                LogInstance.Instance.LogInfo($"Begin download url:[{_downloadUrl}], LocalFullPath:[{ResourceInfo.LocalFullPath}], fileSize:[{ResourceInfo.FileSize}].");

                m_httpClient.Start();
                downloadResult = m_httpClient.DownloadFormServer(_downloadUrl, ResourceInfo.LocalFullPath, ResourceInfo.FileSize);
                if (downloadResult == DownloadResult.NetConnectError)
                {
                    LogInstance.Instance.LogInfo($"localFileName:[{ResourceInfo.LocalFileName}] throw network connect error exception.");
                }
                else
                {
                    LogInstance.Instance.LogInfo($"End download localFileName:[{ResourceInfo.LocalFileName}].");
                }

                ReportDownloadResult(downloadResult);
            }
        }

        private void ReportDownloadResult(DownloadResult downloadResult)
        {
            switch (downloadResult)
            {
                case DownloadResult.NetConnectError:
                    // do nothing
                    break;

                case DownloadResult.UndefineError:
                case DownloadResult.LocalHardDiskNoSpace:
                case DownloadResult.LocalIncorrectPathOrFileName:
                    ChangeDownloadingStatus(DownloadStatus.DownloadFailed);
                    break;

                case DownloadResult.FileNotExist:
                    ChangeDownloadingStatus(DownloadStatus.DownloadFailed_FileNotExist);
                    break;

                case DownloadResult.DownloadPause:
                    ChangeDownloadingStatus(DownloadStatus.DownloadPause);
                    break;

                case DownloadResult.DownloadFinished:
                    ChangeDownloadingStatus(DownloadStatus.DownloadSuccess);
                    break;
            }
        }

        protected override void DoStop()
        {
            m_httpClient.Stop();
        }
    }
}