using System;

namespace DownloadManager.DownloadBase
{
    public sealed class DownloadStatusChangedEventArgs : EventArgs
    {
        public DownloadStatusChangedEventArgs(DownloadWorkItemAbstract resource, DownloadStatus status)
        {
            DownloadResource = resource;
            DownloadStatus = status;
        }

        public DownloadWorkItemAbstract DownloadResource { get; private set; }

        public DownloadStatus DownloadStatus { get; private set; }
    }
}