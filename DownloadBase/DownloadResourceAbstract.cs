namespace DownloadManager.DownloadBase
{
    /// <summary>
    /// Resource File
    /// </summary>
    public abstract class DownloadResourceAbstract
    {
        /// <summary>
        /// databse Primary key unified to string type. (e.g. int, long or GUID)
        /// </summary>
        public abstract string DatabaseID { get; }

        /// <summary>
        /// real resource file ID. ( It might equal to Primary key "DatabaseID" )
        /// </summary>
        public abstract long ResourceID { get; }

        /// <summary>
        /// Preset priority level, 0 is the max.
        /// </summary>
        public abstract int DownloadPriority { get; }

        /// <summary>
        /// User operate immediately downloading, 0 is the min.
        /// </summary>
        public abstract int DownloadImmediatelyPriority { get; }

        /// <summary>
        ///  The file name on disk.
        /// </summary>
        public abstract string LocalFileName { get; }

        /// <summary>
        /// Absolute path on disk. e.g. D:\MyApplication\DownloadFiles
        /// </summary>
        public abstract string StorageDirectory { get; }

        public abstract string DownloadUrl { get; }

        public abstract long FileSize { get; }

        /// <summary>
        /// e.g. 8d43d0df5d0b8b47e73b1067cadb56d6
        /// </summary>
        public abstract string MD5 { get; }

        /// <summary>
        /// MD5 + local file name
        /// </summary>
        public string ResourceKey
        {
            get
            {
                return MD5 + LocalFileName;
            }
        }

        public string LocalFullPath
        {
            get
            {
                return System.IO.Path.Combine(StorageDirectory, LocalFileName);
            }
        }
    }
}