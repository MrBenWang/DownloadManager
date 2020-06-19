namespace DownloadManager.DownloadBase
{
    public interface IDownloadTask<T>
    {
        string ControlName { get; }

        ExecuteResult DownloadImmediately(T _res);

        ExecuteResult DownloadImmediately(long _resourceID, string _url);

        ExecuteResult DownloadStart(T _res);

        ExecuteResult DownloadStart(long _resourceID, string _url);

        ExecuteResult DownloadPause(T _res);

        ExecuteResult DownloadPause(long _resourceID, string _url);

        ExecuteResult DownloadStopAll();
    }
}