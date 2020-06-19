using System;
using System.IO;
using System.Net;
using System.Threading;

namespace DownloadManager.DownloadBase.HTTP
{
    public sealed class HttpClient : DownloadClientAbstract
    {
        public const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Safari/537.36";
        private const int TIME_OUT = 60 * 1000;

        protected override int MAX_BUFFER_LEN
        {
            get
            {
                return 4096;
            }
        }

        protected override object CreateRequest(string _downloadUrl, string _method)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_downloadUrl);
                request.Method = _method;
                request.UserAgent = USER_AGENT;
                request.Proxy = DownloadProxy;
                request.Timeout = TIME_OUT;
                // request.Credentials = new NetworkCredential();
                request.KeepAlive = true;
                return request;
            }
            catch (Exception ex)
            {
                LogInstance.Instance.LogError("Failed to create HttpWebRequest exception: " + ex.ToString());
                throw ex;
            }
        }

        protected override bool GetDownloadFileLength(string _fileUrl, out long fileSize)
        {
            /// One alternative approach to get the file size is to make an HTTP GET call to the server.
            /// response content header (response.Content.Header) with the key "Content-Range"( "bytes 0-15/2328372").

            try
            {
                var req = (HttpWebRequest)CreateRequest(_fileUrl, WebRequestMethods.Http.Head);
                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    fileSize = resp.ContentLength;
                    return true;
                }
            }
            catch (WebException ex)
            {
                LogInstance.Instance.LogError($"GetDownloadFileLength Url:[{_fileUrl}] exception:{ex}");
            }
            catch (Exception ex)
            {
                LogInstance.Instance.LogError($"GetDownloadFileLength Url:[{_fileUrl}] exception exception: {ex}");
            }

            fileSize = 0;
            return false;
        }

        protected override DownloadResult DoDownloading(string _url, string _localFullName, long _offset, long _fullLength)
        {
            Stream reader = null;
            HttpWebRequest request = null;
            FileStream writer = null;
            try
            {
                // begin downloading
                byte[] memBuff = new byte[MAX_BUFFER_LEN];
                request = (HttpWebRequest)CreateRequest(_url, WebRequestMethods.Http.Get);
                request.AddRange(_offset);

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    // if server support HTTP-Range download, response [PartialContent (206)], else response [OK (200)]
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        LogInstance.Instance.LogInfo($"download file:[{_localFullName}] server don't support Range download, set file offset to zero.");
                        _offset = 0;
                    }

                    writer = File.OpenWrite(_localFullName);
                    writer.Seek(_offset, SeekOrigin.Current); // move the pointer in file stream.
                    reader = response.GetResponseStream();
                    int readLen = 0;

                    while (IsDownloading)
                    {
                        for (int i = 0; IsDownloading && i < 30; i++)
                        {
                            if (reader.CanRead)
                            {
                                readLen = reader.Read(memBuff, 0, memBuff.Length);
                                if (readLen > 0 || writer.Length >= _fullLength)
                                {
                                    break;
                                }
                            }

                            // if network congestion or other things, receive 0 byte, then try 30 times in 30 seconds.
                            LogInstance.Instance.LogInfo($"downloading file:[{_localFullName}] read Length <= 0.");
                            Thread.Sleep(1000);
                        }

                        if (writer.Length >= _fullLength)
                        {
                            LogInstance.Instance.LogInfo($"downloading file:[{_localFullName}] download Finished.");
                            break;
                        }
                        else if (readLen <= 0)
                        {
                            return DownloadResult.DownloadPause;
                        }
                        else
                        {
                            writer.Write(memBuff, 0, readLen);
                        }

                        DownloadProgress(_localFullName, writer.Length, _fullLength);
                    }
                }

                var _ret_status = IsDownloading ? DownloadResult.DownloadFinished : DownloadResult.DownloadPause;
                LogInstance.Instance.LogInfo($"downloading file:[{_localFullName}] http client download end, download value is [{_ret_status}].");
                return _ret_status;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    LogInstance.Instance.LogError($"downloading file:[{_localFullName}] downloading protocol error: {ex.Message}");
                    if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        return DownloadResult.FileNotExist;
                    }
                    else
                    {
                        return DownloadResult.UndefineError;
                    }
                }
                else if (ex.Status == WebExceptionStatus.ConnectFailure
                    || ex.Status == WebExceptionStatus.ConnectionClosed
                    || ex.Status == WebExceptionStatus.Timeout
                    || ex.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    LogInstance.Instance.LogError($"downloading file:[{_localFullName}] downloading connect error: {ex.Message}");
                    return DownloadResult.NetConnectError;
                }
                else
                {
                    LogInstance.Instance.LogError($"downloading file:[{_localFullName}] downloading other web exception: {ex.Message}");
                    return DownloadResult.UndefineError;
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                LogInstance.Instance.LogError($"downloading file:[{_localFullName}] downloading socket exception: {ex.Message}, error code: {ex.ErrorCode}, socket error: {ex.SocketErrorCode}.");
                return DownloadResult.NetConnectError;
            }
            catch (Exception ex)
            {
                LogInstance.Instance.LogError($"downloading file:[{_localFullName}] downloading exception: {ex.Message}.");
                return DownloadResult.UndefineError;
            }
            finally
            {
                request?.Abort();
                reader?.Close();
                reader?.Dispose();
                writer?.Close();
                writer?.Dispose();
            }
        }
    }
}