using System.Runtime.Serialization;

namespace DownloadManager.DownloadBase
{
    [DataContract]
    public class WcfResourceKey
    {
        [DataMember]
        public long ResourceID { get; set; }

        [DataMember]
        public string ServerFileName { get; set; }
    }
}