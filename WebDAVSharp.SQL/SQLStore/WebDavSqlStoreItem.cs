using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebDAVSharp.Data;
using WebDAVSharp.Server.Stores;
using WebDAVSharp.Server.Stores.BaseClasses;
using static System.String;
using File = WebDAVSharp.Data.File;

#if DEBUG
using Common.Logging;

#endif

namespace WebDAVSharp.SQL.SQLStore
{
    /// <summary>
    /// </summary>
    public class WebDavSqlStoreItem : WebDavStoreItemBase
    {
#if DEBUG
        internal static readonly ILog Log = LogManager.GetLogger<WebDavSqlStoreItem>();
#endif
        private readonly string _path;

        protected WebDavSqlStoreItem(IWebDavStoreCollection parentCollection, string path, String rootPath, Guid rootGuid, IWebDavStore store)
            : base(parentCollection, path, store)
        {
            if (IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));
            RootGuid = rootGuid;
            RootPath = rootPath;
            _path = path;
            ObjectGuid = GetObjectGuid(path);
            GetFileInfo();
        }

        public override string Etag => ObjectGuid.ToString();
        public override DateTime ModificationDate => GetFileInfo().LastWriteTime ?? DateTime.Now;
        public override DateTime CreationDate => GetFileInfo().CreationTime;
        public Guid? ObjectGuid { get; }
        public string RootPath { get; set; }
        public Guid RootGuid { get; set; }

        public new string Name
        {
            get { return Path.GetFileName(_path); }

            set { throw new InvalidOperationException("Unable to rename item"); }
        }

        public override string ItemPath => _path;
        public override Guid GetRepl_uId() => (Guid) ObjectGuid;

        private Guid? GetObjectGuid(string path)
        {
            //Remove the \\Data
            path = path.Substring(RootPath.Length).Trim();

            using (var context = new OnlineFilesEntities())
            {
                if (path == RootPath)
                    return RootGuid;

                List<string> dirpath = path.Split('\\').ToList();
                while (dirpath.Contains(""))
                    dirpath.Remove("");

                Folder parent = context.Folders.FirstOrDefault(d => d.pk_FolderId == RootGuid);

                if (parent == null)
                    throw new Exception("No Parent");

                Guid? returnValue = parent.pk_FolderId;

                for (int index = 0; index < dirpath.Count; index++)
                {
                    string s = dirpath[index];
                    Folder child = context.Folders.FirstOrDefault(d => d.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase) && d.fk_ParentFolderId == parent.pk_FolderId);
                    if (child != null)
                    {
                        parent = child;
                        returnValue = parent.pk_FolderId;
                    }
                    else
                    {
                        if (index != dirpath.Count - 1)
                        {
                            throw new Exception("Couldn't find folder.");
                        }
                        File file = context.Files.FirstOrDefault(d => d.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase) && d.fk_FolderId == returnValue && !d.IsDeleted);
                        if (file != null)
                            returnValue = file.pk_FileId;
                    }
                }

                return returnValue;
            }
        }

        public override IWebDavFileInfo GetFileInfo()
        {
            throw new NotImplementedException();
        }
    }
}