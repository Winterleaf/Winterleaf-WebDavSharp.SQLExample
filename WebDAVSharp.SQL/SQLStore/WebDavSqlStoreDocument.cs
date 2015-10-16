using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using WebDAVSharp.Data;
using WebDAVSharp.Data.Security;
using WebDAVSharp.Server.Stores;
using File = WebDAVSharp.Data.File;

namespace WebDAVSharp.SQL.SQLStore
{
    /// <summary>
    /// </summary>
    public class WebDavSqlStoreDocument : WebDavSqlStoreItem, IWebDavStoreDocument
    {
        private readonly DateTime _createDate = DateTime.Now;
        private readonly List<byte> _emptyBytes = new List<byte>();
        private readonly long _filesize;
        private readonly DateTime _modificationDate = DateTime.Now;
        private IWebDavFileInfo _fileinfo;

        /// <summary>
        /// </summary>
        /// <param name="parentCollection"></param>
        /// <param name="name"></param>
        /// <param name="rootPath"></param>
        /// <param name="rootGuid"></param>
        /// <param name="store"></param>
        public WebDavSqlStoreDocument(IWebDavStoreCollection parentCollection, string name, String rootPath, Guid rootGuid, IWebDavStore store)
            : base(parentCollection, name, rootPath, rootGuid, store)
        {
            using (var context = new OnlineFilesEntities())
            {
                File file = context.Files.AsNoTracking()
                    .Include(x => x.FileDatas)
                    .FirstOrDefault(d => d.pk_FileId == ObjectGuid && !d.IsDeleted);

                if (file == null)
                    throw new Exception("Non existant file.");
                _createDate = file.CreateDt;

                FileData lastmod = file.FileDatas.OrderByDescending(d => d.Revision).FirstOrDefault();

                _modificationDate = lastmod?.CreateDt ?? file.CreateDt;
                _filesize = lastmod?.Size ?? 1;
            }
        }

        public override long Size => _filesize;
        public override DateTime CreationDate => _createDate;

        /// <summary>
        ///     Gets the modification date of this <see cref="IWebDavStoreItem" />.
        /// </summary>
        public override DateTime ModificationDate => _modificationDate;

        public override bool IsCollection => false;

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Stream OpenReadStream()
        {
            using (var context = new OnlineFilesEntities())
            {
                File file = context.Files.FirstOrDefault(d => d.pk_FileId == ObjectGuid);
                if (file == null)
                    throw new Exception("File Object Not Found.");
                return file.OpenReadStream(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="append"></param>
        /// <returns></returns>
        public Stream OpenWriteStream(bool append)
        {
            if (append)
                throw new Exception("File Stream Append Not supported.");

            using (var context = new OnlineFilesEntities())
            {
                File f = context.Files.FirstOrDefault(d => d.pk_FileId == ObjectGuid);
                return f?.OpenWriteStream(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), ItemPath);
            }
        }

        public override IWebDavFileInfo GetFileInfo()
        {
            if (_fileinfo != null)
                return _fileinfo;

            using (var context = new OnlineFilesEntities())
            {
                File file = context.Files.AsNoTracking().Include(x => x.FileDatas).FirstOrDefault(d => d.pk_FileId == ObjectGuid);
                if (file == null)
                    return new WebDaveSqlStoreFileInfo
                    {
                        Parent = ParentCollection,
                        Path = string.Empty,
                        Exists = false,
                        Directory = false
                    };

                _fileinfo = new WebDaveSqlStoreFileInfo(file.GetFileInfo(), ParentCollection, ItemPath);

                return _fileinfo;
            }
        }
    }
}