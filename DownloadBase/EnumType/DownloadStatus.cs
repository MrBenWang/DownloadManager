namespace DownloadManager.DownloadBase
{
    public enum DownloadStatus
    {
        NotStart = 0,
        WaitForDownload = 1,
        Downloading = 2,
        DownloadPause = 3,
        DownloadFailed_FileNotExist = 4,
        DownloadFailed = 5,
        MD5Checking = 6,
        DownloadFailed_MD5CheckFailed = 7,
        DownloadSuccess = 8,
        UnKnowm = 9
    }
}