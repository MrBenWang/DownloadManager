using DownloadManager.DownloadBase;
using System;

namespace DownloadManager.MyDownloadA
{
    public sealed class AutoResource : DownloadResourceAbstract
    {
        private object m_obj;

        /// <summary>
        /// change type object to custom type what you want.
        /// </summary>
        public AutoResource(object _obj)
        {
            m_obj = _obj;
        }

        public override string DatabaseID => throw new NotImplementedException();

        public override long ResourceID => throw new NotImplementedException();

        public override int DownloadPriority => throw new NotImplementedException();

        public override int DownloadImmediatelyPriority => throw new NotImplementedException();

        public override string LocalFileName => throw new NotImplementedException();

        public override string StorageDirectory => throw new NotImplementedException();

        public override string DownloadUrl => throw new NotImplementedException();

        public override long FileSize => throw new NotImplementedException();

        public override string MD5 => throw new NotImplementedException();
    }
}