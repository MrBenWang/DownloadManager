namespace DownloadManager.DownloadBase
{
    public enum DownloadResult
    {
        Continue,
        UndefineError,
        FileNotExist,
        NetConnectError,
        DownloadPause,
        DownloadFinished,

        LocalIncorrectPathOrFileName,
        LocalHardDiskNoSpace
    }
}