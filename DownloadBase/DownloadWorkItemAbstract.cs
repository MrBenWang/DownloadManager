using System;
using System.IO;

namespace DownloadManager.DownloadBase
{
    /// <summary>
    /// The Resource with download status, MD5 checking.
    /// </summary>
    public abstract class DownloadWorkItemAbstract
    {
        public DownloadResourceAbstract ResourceInfo { get; private set; }

        public DownloadWorkItemAbstract(DownloadResourceAbstract downloadResource)
        {
            ResourceInfo = downloadResource;
        }

        /// <summary>
        /// The status changed in downloading
        /// </summary>
        public event EventHandler<DownloadStatusChangedEventArgs> OnDownloadingStatusChanged;

        protected void ChangeDownloadingStatus(DownloadStatus _status)
        {
            OnDownloadingStatusChanged.Invoke(this, new DownloadStatusChangedEventArgs(this, _status));
        }

        public bool IsDownloading { get; set; }

        public DownloadStatus ResourceDownloadStatus { get; private set; }

        public bool CheckMD5()
        {
            string fileName = string.Empty;
            string localMD5 = string.Empty;
            try
            {
                fileName = Path.Combine(ResourceInfo.StorageDirectory, ResourceInfo.LocalFileName);
                localMD5 = GetMd5Hash(fileName);

                LogInstance.Instance.LogInfo($"begin check md5, file:[{fileName}], localMD5:[{localMD5}], serverMD5:[{ResourceInfo.MD5}].");
                if (!string.Equals(localMD5, ResourceInfo.MD5, StringComparison.OrdinalIgnoreCase))
                {
                    LogInstance.Instance.LogInfo($"check md5 failed, file:[{fileName}], localMD5:[{localMD5}], serverMD5:[{ResourceInfo.MD5}].");
                    if (File.Exists(fileName))
                    {
                        try
                        {
                            LogInstance.Instance.LogError(string.Format("Delete file:[{0}].", fileName));
                            File.Delete(fileName);
                        }
                        catch (Exception ex)
                        {
                            LogInstance.Instance.LogError(string.Format("Delete file:[{0}], Exception:{1}", fileName, ex.ToString()));
                        }
                    }
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogInstance.Instance.LogError($"check md5 failed, file:[{fileName}], localMD5:[{localMD5}], serverMD5:[{ResourceInfo.MD5}], Exception:{ex.ToString()}");
                return false;
            }
        }

        private string GetMd5Hash(string _filename)
        {
            try
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    using (var stream = File.OpenRead(_filename))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "");
                    }
                }
            }
            catch (Exception ex)
            {
                LogInstance.Instance.LogError(string.Format("Get MD5 code failed! File:[{0}], Exception:[{1}].", _filename, ex.ToString()));
                return string.Empty;
            }
        }

        public ExecuteResult DownloadStart()
        {
            if (IsDownloading)
            {
                return ExecuteResult.Success;
            }

            IsDownloading = true;
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                DoStart();
            });
            return ExecuteResult.Success;
        }

        public ExecuteResult DownloadStop()
        {
            if (!IsDownloading)
            {
                return ExecuteResult.Success;
            }

            IsDownloading = false;
            DoStop();
            return ExecuteResult.Success;
        }

        protected abstract void DoStart();

        protected abstract void DoStop();
    }
}