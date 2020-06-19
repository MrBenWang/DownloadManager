using DownloadManager.DownloadBase;
using DownloadManager.MyDownloadA;
using System.Collections.Generic;

namespace DownloadManager
{
    /// <summary>
    /// local database e.g. Firebird or SQLite.
    /// </summary>
    public class LocalDataStorage
    {
        private static LocalDataStorage m_Storage = null;

        public static LocalDataStorage Current
        {
            get
            {
                if (m_Storage == null)
                {
                    m_Storage = new LocalDataStorage();
                }

                return m_Storage;
            }
        }

        #region Database AutoResource

        public AutoResource GetNextWairForDownloadAutoResource()
        {
            /// example LINQ:
            /// context.xxx.Where(m => m.download_status == (int)DownloadStatus.WaitForDownload).OrderByDescending(m => m.immdeiately_priority).OrderBy(m => m.download_priority).FirstOrDefault();
            return new AutoResource(null);
        }

        public ExecuteResult UpdateAutoResourceStatus(string _dbId, DownloadStatus _status)
        {
            /// example LINQ:
            /// var _entity = context.xxx.Where(m => m.id == (int)_dbId).First();
            /// _entity.download_status = (int)_status;
            return ExecuteResult.Success;
        }

        /// <summary>
        /// get last time application shutdown, those Downloading or MD5Checking resources.
        /// prepare continue downloading.
        /// </summary>
        /// <returns></returns>
        public List<AutoResource> GetAllDownloadingAutoResources()
        {
            /// example LINQ:
            /// context.xxx.Where(m => m.download_status == (int)DownloadStatus.Downloading || m.download_status == (int)DownloadStatus.MD5Checking);
            return new List<AutoResource>();
        }

        public AutoResource GetAutoResourceByResourceID(long _resourceId, string _url)
        {
            /// example LINQ:
            /// context.xxx.Where(m => m.resourceId == _resourceId && m.download_url == _url).FirstOrDefault()
            return new AutoResource(null);
        }

        /// <summary>
        /// reset download no success status to NotStart
        /// if file status is DownloadSuccess, but not exist, reset to NotStart
        /// </summary>
        /// <returns></returns>
        public ExecuteResult ResetNoSuccessFlashToolStatus()
        {
            /// example LINQ:
            /// var _tmpList = context.xxx.Where(m => m.local_status != (int)DownloadStatus.Downloading);
            /// foreach (var item in _tmpList) {
            ///     if (item.local_status == (int)DownloadStatus.DownloadSuccess) {
            ///         if (!File.Exists(item.local_filename)) item.local_status = (int)DownloadStatus.NotStart;
            ///     } else {
            ///         item.local_status = (int)DownloadStatus.NotStart;
            ///     }
            ///  }
            return ExecuteResult.Success;
        }

        #endregion Database AutoResource

        #region Database UserOperResource

        public ExecuteResult UpdateUserOperResourceStatus(string _dbId, DownloadStatus _status)
        {
            /// example LINQ:
            /// var _entity = context.xxx.Where(m => m.id == (int)_dbId).First();
            /// _entity.download_status = (int)_status;
            return ExecuteResult.Success;
        }

        public UserOperRescource GetUserOperResourceSetHighestPriority(long _resourceId, string _url)
        {
            /// example LINQ:
            /// int _localMax = context.xxx.Max(m => m.immdeiately_priority);
            /// var _target = context.xxx.Where(m => m.resourceId == _resourceId && m.download_url == _url).First();
            /// _target.immdeiately_priority = _localMax + 1;
            return new UserOperRescource(null);
        }

        public UserOperRescource GetUserOperResourceByResourceID(long _resourceId, string _url)
        {
            /// example LINQ:
            /// context.xxx.Where(m => m.resourceId == _resourceId && m.download_url == _url).FirstOrDefault()
            return new UserOperRescource(null);
        }

        #endregion Database UserOperResource
    }
}