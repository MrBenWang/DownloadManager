using System;
using System.IO;
using System.Net;

namespace DownloadManager.DownloadBase
{
    public abstract class DownloadClientAbstract
    {
        private volatile bool is_download;
        public static IWebProxy DownloadProxy { get; private set; }

        protected abstract int MAX_BUFFER_LEN { get; }

        protected bool IsDownloading
        {
            get { return is_download; }
        }

        public DownloadClientAbstract()
        {
            is_download = false;
            SetDownloadProxy(ProxyMode.SystemProxy, null); // set default Proxy is SystemProxy.
        }

        protected abstract object CreateRequest(string _downloadUrl, string _method);

        protected abstract bool GetDownloadFileLength(string serverFile, out long fileSize);

        protected abstract DownloadResult DoDownloading(string _url, string _localFullName, long _offset, long _fullLength);

        public void Start()
        {
            is_download = true;
        }

        public void Stop()
        {
            is_download = false;
        }

        #region Downloading

        public DownloadResult DownloadFormServer(string _url, string _localFullName, long _fileSize)
        {
            if (!CreateDirectory(Path.GetDirectoryName(_localFullName)))
            {
                return DownloadResult.LocalIncorrectPathOrFileName;
            }

            long _fullLength;
            if (!GetDownloadFileLength(_url, out _fullLength))
            {
                return DownloadResult.NetConnectError;
            }
            if (_fileSize != _fullLength)
            {
                LogInstance.Instance.LogError($"Known file:[{_localFullName}] size:[{_fileSize}] don't equal online url:[{_url}] file size:[{_fullLength}].");
            }

            long _offset;
            var _result = GetFileLengthNeedDownload(_url, _localFullName, _fullLength, out _offset);
            if (_result != DownloadResult.Continue)
            {
                return _result;
            }

            return DoDownloading(_url, _localFullName, _offset, _fullLength);
        }

        private bool CreateDirectory(string _dirName)
        {
            try
            {
                // compatible UNC（Universal Naming Convention）path on network.
                if (!Directory.Exists(_dirName))
                {
                    Directory.CreateDirectory(_dirName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogInstance.Instance.LogError($"HttpClient create directory:[{_dirName}] exception:{ex}");
            }

            return false;
        }

        private DownloadResult GetFileLengthNeedDownload(string _url, string _fullPath, long _fullLength, out long _currnetOffset)
        {
            // if file exist, it maybe download pause.
            long _currentLength = 0;
            _currnetOffset = 0;
            if (File.Exists(_fullPath))
            {
                _currentLength = new FileInfo(_fullPath).Length;
                if (_fullLength > _currentLength)
                {
                    _currnetOffset = _currentLength; // continue downloading
                    LogInstance.Instance.LogInfo($"download file:[{_fullPath}] length {_currentLength} completed.");
                }
                else if (_fullLength > 0 && _currentLength == _fullLength)
                {
                    LogInstance.Instance.LogInfo($"download file:[{_fullPath}] already exists, downloading finish.");
                    return DownloadResult.DownloadFinished;
                }
                else if (_fullLength > 0 && _currentLength > _fullLength)
                {
                    _currentLength = 0;
                    LogInstance.Instance.LogInfo($"current file:[{_fullPath}] length:[{_currentLength}] is incorrect(net file length:[{_fullLength}]), delete current file, download again.");
                }
            }

            long _needdownloadLen = _fullLength - _currentLength;
            if (!CheckFreeSpaceOnDisk(_needdownloadLen, Path.GetPathRoot(_fullPath)))
            {
                LogInstance.Instance.LogError("Hard disk has not enough free space for resource waiting for download.★★★★★");
                return DownloadResult.LocalHardDiskNoSpace;
            }

            return DownloadResult.Continue;
        }

        private bool CheckFreeSpaceOnDisk(long fileLen, string driveId)
        {
            if (new Uri(driveId, UriKind.Absolute).IsUnc)
            {
                // UNC（Universal Naming Convention）path can be used to access network resources, like: \\192.168.1.100\MyDir
                // don't check free space
                return true;
            }
            else
            {
                // keep 100M free space at least.
                System.IO.DriveInfo drive = new System.IO.DriveInfo(driveId);
                return drive.AvailableFreeSpace > (fileLen + 100 * 1024 * 1024);
            }
        }

        #endregion Downloading

        public void SetDownloadProxy(ProxyMode _mode, ProxyModel _proxy)
        {
            switch (_mode)
            {
                case ProxyMode.NotUseProxy:
                    DownloadProxy = null;
                    break;

                case ProxyMode.SystemProxy:
                    DownloadProxy = WebRequest.GetSystemWebProxy();
                    break;

                case ProxyMode.CustomProxy:
                    DownloadProxy = new WebProxy(_proxy.ProxyIP, _proxy.ProxyPort);
                    DownloadProxy.Credentials = new NetworkCredential(_proxy.ProxyUser, _proxy.ProxyPwd);
                    break;
            }
        }

        private int _percent = 0;

        protected void DownloadProgress(string _fileFullName, long _receiveByte, long _totalByte)
        {
            if (_totalByte > 0)
            {
                int newPercent = (int)((_receiveByte * 100) / _totalByte);
                if (_percent != newPercent)
                {
                    _percent = newPercent;
                    LogInstance.Instance.LogInfo($"{Path.GetFileName(_fileFullName)} download {newPercent}% , download size = {_receiveByte}, fileLength = {_totalByte}!");
                }
            }
        }
    }
}