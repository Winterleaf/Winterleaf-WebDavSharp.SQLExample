using WebDAVSharp.Data.HelperClasses;
using WebDAVSharp.Server.Stores;

namespace WebDAVSharp.SQL.SQLStore
{
    public class WebDaveSqlStoreFileInfo : SqlStoreFileInfo
    {
        public WebDaveSqlStoreFileInfo()
        {
        }

        public WebDaveSqlStoreFileInfo(SqlStoreFileInfo fileInfo, IWebDavStoreCollection parent, string path)
        {
            ObjectGuid = fileInfo.ObjectGuid;
            Archive = fileInfo.Archive;
            Compressed = fileInfo.Compressed;
            CreationTime = fileInfo.CreationTime;
            Device = fileInfo.Device;
            Directory = fileInfo.Directory;
            Encrypted = fileInfo.Encrypted;
            Exists = fileInfo.Exists;
            Hidden = fileInfo.Hidden;
            IntegrityStream = fileInfo.IntegrityStream;
            LastAccessTime = fileInfo.LastAccessTime;
            LastWriteTime = fileInfo.LastWriteTime;
            NoScrubData = fileInfo.NoScrubData;
            Normal = fileInfo.Normal;
            NotContentIndexed = fileInfo.NotContentIndexed;
            Offline = fileInfo.Offline;
            Parent = parent;
            Path = path;
            ReadOnly = fileInfo.ReadOnly;
            ReparsePoint = fileInfo.ReparsePoint;
            SparseFile = fileInfo.SparseFile;
            System = fileInfo.System;
            Temporary = fileInfo.Temporary;
        }

        public override void Apply()
        {
            base.Apply();
            if (Directory)
                WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(Path);
            else
            {
                WebDavSqlStoreDocumentFactory.Instance.InvalidateDocumentPath(Path);
                WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(Parent.ItemPath);
            }
        }
    }
}