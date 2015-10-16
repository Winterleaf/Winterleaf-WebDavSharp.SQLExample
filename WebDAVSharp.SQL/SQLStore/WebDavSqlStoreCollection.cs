using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security;
using WebDAVSharp.Data;
using WebDAVSharp.Data.Security;
using WebDAVSharp.Data.SQLObjects;
using WebDAVSharp.Server.Exceptions;
using WebDAVSharp.Server.Stores;
using File = WebDAVSharp.Data.File;

namespace WebDAVSharp.SQL.SQLStore
{
    /// <summary>
    /// </summary>
    public class WebDavSqlStoreCollection : WebDavSqlStoreItem, IWebDavStoreCollection
    {
        private readonly DateTime _creationDt;
        private readonly IWebDavFileInfo _fileinfo = null;
        private List<IWebDavStoreItem> _items;

        /// <summary>
        /// </summary>
        /// <param name="parentCollection"></param>
        /// <param name="path"></param>
        /// <param name="rootPath"></param>
        /// <param name="rootGuid"></param>
        /// <param name="store"></param>
        public WebDavSqlStoreCollection(IWebDavStoreCollection parentCollection, string path, String rootPath, Guid rootGuid, IWebDavStore store)
            : base(parentCollection, path, rootPath, rootGuid, store)
        {
            using (var context = new OnlineFilesEntities())
            {
                Folder f = context.Folders.AsNoTracking().FirstOrDefault(d => d.pk_FolderId == ObjectGuid);
                if (f != null)
                    _creationDt = f.CreateDt;
            }
        }

        public IEnumerable<IWebDavStoreItem> Items => _items ?? (_items = GetItems());

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IWebDavStoreItem GetItemByName(string name)
        {
            string path = Path.Combine(ItemPath, name);
#if DEBUG
            Log.Warn("Parent: " + ObjectGuid + " - Requesting Item by name: " + path);
#endif
            using (var context = new OnlineFilesEntities())
            {
                Folder folder = context.Folders.AsNoTracking().FirstOrDefault(d =>
                    d.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                    d.fk_ParentFolderId == ObjectGuid &&
                    !d.IsDeleted
                    );
                if (folder != null)
                    return WebDavSqlStoreCollectionFactory.Instance.GetCollection(this, Path.Combine(ItemPath, folder.Name), RootPath, RootGuid);

                File file = context.Files.AsNoTracking().FirstOrDefault(d => d.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                                                                             && d.fk_FolderId == ObjectGuid && !d.IsDeleted);
                if (file != null)
                    return WebDavSqlStoreDocumentFactory.Instance.GetDocument(this, Path.Combine(ItemPath, file.Name), RootPath, RootGuid);
            }

            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IWebDavStoreCollection CreateCollection(string name)
        {
#if DEBUG
            Log.Info("Creating Folder: " + name);
#endif
            Folder.Create(name, ObjectGuid, PrincipleFactory.Instance.GetPrinciple(FromType.WebDav).UserProfile);
#if DEBUG
            Log.Info("Folder Created.");
#endif

            var col = GetItemByName(name) as IWebDavStoreCollection;
            WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(col.ItemPath);
            WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(ItemPath);
            return col;
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        public void Delete(IWebDavStoreItem item)
        {
#if DEBUG
            Log.Info("Deleting Item: " + item.Name);
#endif
            if (IsCheckedOut(item))
                throw new Exception("Item is checked out.");

            using (var context = new OnlineFilesEntities())
            {
                var collection = item as WebDavSqlStoreCollection;
                if (collection != null)
                {
                    Folder folder = context.Folders.FirstOrDefault(d => d.pk_FolderId == collection.ObjectGuid);
                    if (folder == null)
                        throw new WebDavNotFoundException("Folder Not Found.");
                    folder.SetDeleted(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav).UserProfile);
                    context.SaveChanges();
                    WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(item.ItemPath);
                    WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(ItemPath);
                }
                else
                {
                    WebDavSqlStoreDocument document = item as WebDavSqlStoreDocument;
                    if (document == null)
                        return;
                    var doc = document;
                    File file = context.Files.FirstOrDefault(d => d.pk_FileId == doc.ObjectGuid);
                    if (file == null)
                        throw new WebDavNotFoundException("Folder Not Found.");
                    file.SetDeleted(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav).UserProfile);
                    context.SaveChanges();
                    WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(ItemPath);
                    WebDavSqlStoreDocumentFactory.Instance.InvalidateDocumentPath(doc.ItemPath);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IWebDavStoreDocument CreateDocument(string name)
        {
            File.Create(ObjectGuid, name, PrincipleFactory.Instance.GetPrinciple(FromType.WebDav).UserProfile);
            WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(ItemPath);
            WebDavSqlStoreDocumentFactory.Instance.InvalidateDocumentPath(Path.Combine(ItemPath, name));
            return WebDavSqlStoreDocumentFactory.Instance.GetDocument(this, Path.Combine(ItemPath, name), RootPath, RootGuid);
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destinationName"></param>
        /// <param name="includeContent"></param>
        /// <returns></returns>
        public IWebDavStoreItem CopyItemHere(IWebDavStoreItem source, string destinationName, bool includeContent)
        {
            IWebDavStoreItem returnitem;
            using (var context = new OnlineFilesEntities())
            {
                var sourceFolder = source as WebDavSqlStoreCollection;
                if (sourceFolder != null)
                {
                    Folder targetFolder = context.Folders.FirstOrDefault(d => d.pk_FolderId == sourceFolder.ObjectGuid);
                    if (targetFolder == null)
                        return null;

                    Folder destination = context.Folders.FirstOrDefault(d => d.pk_FolderId == ObjectGuid);
                    destination.CopyFolderHere(targetFolder.pk_FolderId, destinationName, PrincipleFactory.Instance.GetPrinciple(FromType.WebDav));
                    context.SaveChanges();
                    WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(ItemPath);
                    WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(sourceFolder.ItemPath);
                    WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(Path.Combine(ItemPath, destinationName));
                    returnitem = WebDavSqlStoreCollectionFactory.Instance.GetCollection(this, Path.Combine(ItemPath, destinationName), RootPath, RootGuid);
                }
                else
                {
                    WebDavSqlStoreDocument document = source as WebDavSqlStoreDocument;
                    if (document == null)
                        return null;
                    WebDavSqlStoreDocument doc = document;
                    var destFolder = context.Folders.FirstOrDefault(d => d.pk_FolderId == ObjectGuid);
                    destFolder.CopyFileHere((Guid) document.ObjectGuid, destinationName, PrincipleFactory.Instance.GetPrinciple(FromType.WebDav));
                    WebDavSqlStoreDocumentFactory.Instance.InvalidateDocumentPath(Path.Combine(ItemPath, destinationName));
                    WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(ItemPath);
                    returnitem = WebDavSqlStoreDocumentFactory.Instance.GetDocument(this, Path.Combine(ItemPath, destinationName), RootPath, RootGuid);
                }
            }
            return returnitem;
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destinationName"></param>
        /// <returns></returns>
        public IWebDavStoreItem MoveItemHere(IWebDavStoreItem source, string destinationName)
        {
            //this -> is where it wants to be moved to.
            //source is the item
            //destination name is the name they want it to be.
            IWebDavStoreItem returnitem;
            using (var context = new OnlineFilesEntities())
            {
                var sourceFolder = source as WebDavSqlStoreCollection;
                if (sourceFolder != null)
                {
                    Folder targetFolder = context.Folders.FirstOrDefault(d => d.pk_FolderId == sourceFolder.ObjectGuid);
                    if (targetFolder == null)
                        return null;

                    Folder destination = context.Folders.FirstOrDefault(d => d.pk_FolderId == ObjectGuid);
                    destination.MoveFolderHere(targetFolder.pk_FolderId, destinationName, PrincipleFactory.Instance.GetPrinciple(FromType.WebDav));
                    context.SaveChanges();
                    WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(ItemPath);
                    WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(sourceFolder.ItemPath);
                    WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(Path.Combine(ItemPath, destinationName));
                    returnitem = WebDavSqlStoreCollectionFactory.Instance.GetCollection(this, Path.Combine(ItemPath, destinationName), RootPath, RootGuid);
                }
                else
                {
                    WebDavSqlStoreDocument document = source as WebDavSqlStoreDocument;
                    if (document == null)
                        return null;
                    WebDavSqlStoreDocument doc = document;
                    var destFolder = context.Folders.FirstOrDefault(d => d.pk_FolderId == ObjectGuid);
                    destFolder.MoveFileHere((Guid) document.ObjectGuid, destinationName, PrincipleFactory.Instance.GetPrinciple(FromType.WebDav));
                    WebDavSqlStoreDocumentFactory.Instance.InvalidateDocumentPath(Path.Combine(ItemPath, destinationName));
                    WebDavSqlStoreCollectionFactory.Instance.InvalidateCollection(ItemPath);
                    returnitem = WebDavSqlStoreDocumentFactory.Instance.GetDocument(this, Path.Combine(ItemPath, destinationName), RootPath, RootGuid);
                }
            }
            return returnitem;
        }

        public override IWebDavFileInfo GetFileInfo()
        {
            if (_fileinfo != null)
                return _fileinfo;

            using (var context = new OnlineFilesEntities())
            {
                Folder folder = context.Folders.AsNoTracking()
                    .FirstOrDefault(d => d.pk_FolderId == ObjectGuid);
                if (folder == null)
                    return new WebDaveSqlStoreFileInfo
                    {
                        Parent = ParentCollection,
                        Path = "",
                        Exists = false,
                        Directory = true
                    };
                return new WebDaveSqlStoreFileInfo
                {
                    Parent = ParentCollection,
                    Path = ItemPath,
                    Exists = true,
                    CreationTime = folder.CreateDt,
                    LastAccessTime = folder.CreateDt,
                    LastWriteTime = folder.CreateDt,
                    Archive = folder.GetWin32Attribute(FileAttributes.Archive),
                    Compressed = folder.GetWin32Attribute(FileAttributes.Compressed),
                    Device = folder.GetWin32Attribute(FileAttributes.Device),
                    Directory = true,
                    Hidden = folder.GetWin32Attribute(FileAttributes.Hidden),
                    Encrypted = folder.GetWin32Attribute(FileAttributes.Encrypted),
                    IntegrityStream = folder.GetWin32Attribute(FileAttributes.IntegrityStream),
                    NoScrubData = folder.GetWin32Attribute(FileAttributes.NoScrubData),
                    Normal = folder.GetWin32Attribute(FileAttributes.Normal),
                    NotContentIndexed = folder.GetWin32Attribute(FileAttributes.NotContentIndexed),
                    Offline = folder.GetWin32Attribute(FileAttributes.Offline),
                    ReadOnly = folder.GetWin32Attribute(FileAttributes.ReadOnly),
                    ReparsePoint = folder.GetWin32Attribute(FileAttributes.ReparsePoint),
                    SparseFile = folder.GetWin32Attribute(FileAttributes.SparseFile),
                    System = folder.GetWin32Attribute(FileAttributes.System),
                    Temporary = folder.GetWin32Attribute(FileAttributes.Temporary),
                    ObjectGuid = ObjectGuid
                };
            }
        }

        /// <summary>
        /// </summary>
        public override long Size => 0;

        /// <summary>
        /// </summary>
        public override string MimeType => "application/octet-stream";

        public override bool IsCollection => true;
        public override DateTime CreationDate => _creationDt;
        public override DateTime ModificationDate => _creationDt;

        public bool IsCheckedOut(IWebDavStoreItem item)
        {
            using (var context = new OnlineFilesEntities())
            {
                List<SpDoAnyChildrenHaveLocksResult> cellData =
                    context.Database.SqlQuery<SpDoAnyChildrenHaveLocksResult>
                        ($"dbo.sp_DoAnyChildrenHaveLocks '{((WebDavSqlStoreItem) item).ObjectGuid}'").ToList();
                if (cellData.Any())
                {
                    if (cellData[0].Exists)
                        return true;
                }
                else
                    throw new WebDavNotFoundException("Shouldn't get here.");
                return false;
            }
        }

        public List<IWebDavStoreItem> GetItems()
        {
            List<IWebDavStoreItem> items = new List<IWebDavStoreItem>();

            using (var context = new OnlineFilesEntities())
            {
                var currentFolder = context.Folders
                    .Include(c => c.Files)
                    .Include(c => c.ChildFolders)
                    .FirstOrDefault(d => d.pk_FolderId == ObjectGuid);

                if (currentFolder == null)
                    return items;

                Principal p = PrincipleFactory.Instance.GetPrinciple(FromType.WebDav);

                try
                {
                    items.AddRange(context.GetChildFolders(ObjectGuid, p, true).Select(folder => WebDavSqlStoreCollectionFactory.Instance.GetCollection(this, Path.Combine(ItemPath, folder.Name), RootPath, RootGuid)));
                }
                catch (SecurityException)
                {
                }
                try
                {
                    items.AddRange(context.GetChildFiles(ObjectGuid, p, true).Select(file => WebDavSqlStoreDocumentFactory.Instance.GetDocument(this, Path.Combine(ItemPath, file.Name), RootPath, RootGuid)));
                }
                catch (SecurityException)
                {
                }
            }


            return items;
        }
    }
}