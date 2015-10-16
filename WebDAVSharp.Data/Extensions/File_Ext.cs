using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Security;
using System.Web;
using WebDAVSharp.Data.Enums;
using WebDAVSharp.Data.Extensions;
using WebDAVSharp.Data.HelperClasses;
using WebDAVSharp.Data.Security;
using WebDAVSharp.Data.SQLObjects;
using WebDAVSharp.Server.Exceptions;

namespace WebDAVSharp.Data
{
    public static class FileEx
    {
        /// <summary>
        ///     Validates that the file name is valid.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="filename"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string CheckFileName(this Folder parent, string filename, OnlineFilesEntities context = null)
        {
            return filename;
        }

        /// <summary>
        ///     Checks to see if the file is checked out by anyone
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsCheckedOut(this File file)
        {
            using (var context = new OnlineFilesEntities())
            {
                List<SpDoAnyChildrenHaveLocksResult> cellData =
                    context.Database.SqlQuery<SpDoAnyChildrenHaveLocksResult>($"dbo.sp_DoAnyChildrenHaveLocks '{file.pk_FileId}'").ToList();
                if (cellData.Any())
                {
                    if (cellData[0].Exists)
                        return true;
                }
                else
                    throw new WebDavNotFoundException("Shouldn't get here.");
            }
            return false;
        }
    }

    public partial class File : IFileSystemObject
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly List<byte> _emptyBytes = new List<byte>();

        public long FileSize
        {
            get
            {
                using (var context = new OnlineFilesEntities())
                {
                    var t = context.FileDatas.OrderByDescending(s => s.Revision).First(d => d.fk_FileId == pk_FileId);
                    return t?.Size ?? 0;
                }
            }
        }

        public SqlStoreFileInfo DosFileInfo => GetFileInfo();
        public FileSystemObjectType ObjectType => FileSystemObjectType.File;
        public Guid ParentObject => fk_FolderId;
        public Guid FileSystemId => pk_FileId;
        public string DisplayName => Name;
        public DateTime CreatedDate => CreateDt;
        public DateTime? DeletedDate => DeletedDt;
        public bool ObjectIsDeleted => IsDeleted;
        public SecurityObject DeletedBySecurityObject => DeletedBy;
        public SecurityObject OwnerSecurityObject => Owner;
        public SecurityObject CreatedBySecurityObject => CreatedBy;
        public string FileMimeType => MimeType;
        public bool? FileIsRevisioned => isRevisioned;
        public Guid? FolderCatalogCollectionId => null;

        public IFileSystemObject GetInfo()
        {
            return this;
        }

        /// <summary>
        ///     Returns the bit flag to a file attribute.
        /// </summary>
        /// <param name="attrib"></param>
        /// <returns></returns>
        public bool GetWin32Attribute(FileAttributes attrib)
        {
            return FlagsHelper.IsSet((FileAttributes) Win32FileAttribute, attrib);
        }

        /// <summary>
        ///     Sets a bit flag to a file attribute.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="attrib"></param>
        /// <param name="value"></param>
        public void SetWin32Attribute(Principal user, FileAttributes attrib, bool value)
        {
            using (var context = new OnlineFilesEntities())
                if (!(context.FileSecurities.Where(d => d.fk_FileId == pk_FileId).ToList().Any(x => user.UserProfile.mySecurityGroups.Contains(x.SecurityObjectId) && x.canWrite)))
                    throw new SecurityException("Not Authorized.");
            if (value)
                Win32FileAttribute = (int) FlagsHelper.Set(((FileAttributes) Win32FileAttribute), attrib);
            else
                Win32FileAttribute = (int) FlagsHelper.Unset(((FileAttributes) Win32FileAttribute), attrib);
        }

        /// <summary>
        ///     Opens a READ only stream to the file data.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Stream OpenReadStream(Principal user)
        {
            using (var context = new OnlineFilesEntities())
            {
                var sec = context.FileSecurities.Where(d => d.fk_FileId == pk_FileId && user.UserProfile.mySecurityGroups.Contains(d.SecurityObjectId));
                if (!sec.Any(d => d.canRead))
                    throw new SecurityException("Not Authorized.");

                FileData filedata = context.FileDatas.AsNoTracking()
                    .Include(x => x.Catalog)
                    .Where(d => d.fk_FileId == pk_FileId)
                    .OrderByDescending(d => d.Revision)
                    .FirstOrDefault();

                if (filedata == null)
                    return new MemoryStream(_emptyBytes.ToArray())
                    {
                        Position = 0
                    };

                using (var ctx = new OnlineFiles_CatalogEntities(filedata.Catalog.EntityConnectionString))
                {
                    FileCatalogEntry filecat = ctx.FileCatalogEntries.AsNoTracking()
                        .FirstOrDefault(d => d.pk_FileCatalogEntryId == filedata.fk_ContentId);

                    if (filecat == null)
                        return new MemoryStream(_emptyBytes.ToArray())
                        {
                            Position = 0
                        };
                    return new MemoryStream(filecat.binaryData)
                    {
                        Position = 0
                    };
                }
            }
        }

        /// <summary>
        ///     Opens a WRITE stream to the file.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="itemPath"></param>
        /// <param name="webDavSqlStoreDocumentFactoryInstance"></param>
        /// <returns></returns>
        public Stream OpenWriteStream(Principal user, string itemPath = null, object webDavSqlStoreDocumentFactoryInstance = null)
        {
            using (var context = new OnlineFilesEntities())
            {
                if (!(context.FileSecurities.Where(d => d.fk_FileId == pk_FileId).ToList().Any(x => user.UserProfile.mySecurityGroups.Contains(x.SecurityObjectId) && x.canWrite)))
                    throw new SecurityException("Not Authorized.");


                int revision = 0;

                FileData fd = context.FileDatas.Include(x => x.Catalog).Where(d => d.fk_FileId == pk_FileId).OrderByDescending(d => d.Revision).FirstOrDefault();
                if (fd != null)
                    revision = fd.Revision;

                revision++;


                Catalog catalog;
                if (fd == null || fd.Catalog.fk_CatalogStatusId != CatalogStatus.Open)
                {
                    Folder f = context.Folders.FirstOrDefault(d => d.pk_FolderId == fk_FolderId);
                    if (f == null)
                        throw new Exception("Null ptr");

                    CatalogCollection t = context.CatalogCollections.Include(d => d.Catalogs).FirstOrDefault(d => d.pk_CatalogCollectionId == f.fk_CatalogCollectionId);
                    if (t == null)
                        throw new Exception("Cat col is null");
                    catalog = t.Catalogs.FirstOrDefault(d => d.fk_CatalogStatusId == CatalogStatus.Open);
                    if (catalog == null)
                        throw new Exception("No Catalog Available.");
                }
                else
                    catalog = fd.Catalog;


                if (catalog == null)
                    throw new Exception("No Catalog Available for file.");

                using (var ctx = new OnlineFiles_CatalogEntities(catalog.EntityConnectionString))
                {
                    FileCatalogEntry fce = new FileCatalogEntry
                    {
                        binaryData = _emptyBytes.ToArray()
                    };
                    ctx.FileCatalogEntries.Add(fce);

                    ctx.SaveChanges();

                    FileData filedata = new FileData
                    {
                        fk_FileId = pk_FileId,
                        Revision = revision,
                        Size = 0,
                        CreateDt = DateTime.Now,
                        fk_CatalogId = catalog.pk_CatalogId,
                        fk_ContentId = fce.pk_FileCatalogEntryId
                    };

                    context.FileDatas.Add(filedata);

                    context.SaveChanges();

                    Stream stream = new SqlStoreFileStream
                    {
                        CatalogId = catalog.pk_CatalogId,
                        FileCatalogEntryId = fce.pk_FileCatalogEntryId,
                        Path = itemPath,
                        FileDataId = filedata.pk_FileDataId,
                        WebDavSqlStoreDocumentFactoryInstance = webDavSqlStoreDocumentFactoryInstance
                    };

                    return stream;
                }
            }
        }

        /// <summary>
        ///     Marks the File object as deleted, don't forget to commitchanges.
        /// </summary>
        /// <param name="deletedBy"></param>
        public void SetDeleted(SecurityObject deletedBy)
        {
            using (var context = new OnlineFilesEntities())
                if (!(context.FileSecurities.AsNoTracking().Any(x => deletedBy.mySecurityGroups.Contains(x.SecurityObjectId) && x.CanDelete)))
                    throw new SecurityException("Not Authorized.");

            IsDeleted = true;
            DeletedDt = DateTime.Now;
            DeletedById = deletedBy.SecurityObjectId;
        }

        /// <summary>
        ///     Marks the File object as deleted, don't forget to commitchanges.
        /// </summary>
        /// <param name="deletedBy"></param>
        public void SetDeleted(Principal deletedBy)
        {
            using (var context = new OnlineFilesEntities())
                if (!(context.FileSecurities.AsNoTracking().Any(x => deletedBy.UserProfile.mySecurityGroups.Contains(x.SecurityObjectId) && x.CanDelete)))
                    throw new SecurityException("Not Authorized.");

            IsDeleted = true;
            DeletedDt = DateTime.Now;
            DeletedById = deletedBy.UserProfile.SecurityObjectId;
        }

        public void RestoreDeleted(SecurityObject undeletedBy)
        {
            using (var context = new OnlineFilesEntities())
                if (!(context.FileSecurities.AsNoTracking().Any(x => undeletedBy.mySecurityGroups.Contains(x.SecurityObjectId) && x.CanDelete)))
                    throw new SecurityException("Not Authorized.");

            IsDeleted = false;
            DeletedDt = DateTime.Now;
            DeletedById = undeletedBy.SecurityObjectId;
        }

        /// <summary>
        ///     Returns the FileInfo
        /// </summary>
        /// <returns></returns>
        public SqlStoreFileInfo GetFileInfo()
        {
            FileData lastdata;
            if (FileDatas == null)
                using (var context = new OnlineFilesEntities())
                    lastdata = context.FileDatas.OrderByDescending(d => d.Revision).FirstOrDefault();
            else
                lastdata = FileDatas.OrderByDescending(d => d.Revision).FirstOrDefault();

            var fileinfo = new SqlStoreFileInfo
            {
                Parent = null,
                Path = null,
                Exists = true,
                CreationTime = CreateDt,
                LastAccessTime = CreateDt,
                LastWriteTime = lastdata?.CreateDt ?? CreateDt,
                Directory = false,
                Archive = GetWin32Attribute(FileAttributes.Archive),
                Compressed = GetWin32Attribute(FileAttributes.Compressed),
                Device = GetWin32Attribute(FileAttributes.Device),
                Encrypted = GetWin32Attribute(FileAttributes.Encrypted),
                NotContentIndexed = GetWin32Attribute(FileAttributes.NotContentIndexed),
                Offline = GetWin32Attribute(FileAttributes.Offline),
                System = GetWin32Attribute(FileAttributes.System),
                Hidden = GetWin32Attribute(FileAttributes.Hidden),
                IntegrityStream = GetWin32Attribute(FileAttributes.IntegrityStream),
                NoScrubData = GetWin32Attribute(FileAttributes.NoScrubData),
                Normal = GetWin32Attribute(FileAttributes.Normal),
                ReadOnly = GetWin32Attribute(FileAttributes.ReadOnly),
                ReparsePoint = GetWin32Attribute(FileAttributes.ReparsePoint),
                SparseFile = GetWin32Attribute(FileAttributes.SparseFile),
                Temporary = GetWin32Attribute(FileAttributes.Temporary),
                ObjectGuid = pk_FileId
            };
            return fileinfo;
        }

        /// <summary>
        ///     Creates a new File Object.
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="name"></param>
        /// <param name="createdBy"></param>
        /// <param name="inheritSecurity"></param>
        /// <returns></returns>
        public static File Create(Guid? folderId, string name, SecurityObject createdBy, bool inheritSecurity = true)
        {
            using (var context = new OnlineFilesEntities())
            {
                if (folderId == null)
                    throw new Exception("Bad Guid.");

                var folder = context.Folders
                    .Include(x => x.FolderSecurities)
                    .FirstOrDefault(d => d.pk_FolderId == folderId);
                if (folder == null)
                    throw new Exception("Folder Not Found.");

                if (!folder.FolderSecurities.Any(x => createdBy.mySecurityGroups.Contains(x.SecurityObjectId) && x.canCreateFiles))
                    throw new SecurityException("No Access.");


                var file = new File
                {
                    fk_FolderId = (Guid) folderId,
                    IsDeleted = false,
                    isRevisioned = true,
                    Name = name,
                    MimeType = MimeMapping.GetMimeMapping(name),
                    CreatedById = createdBy.SecurityObjectId,
                    CreateDt = DateTime.Now,
                    OwnerId = createdBy.SecurityObjectId
                };
                context.Files.Add(file);
                context.SaveChanges();

                FileSecurity fileSecurity = new FileSecurity
                {
                    CanDelete = true,
                    canRead = true,
                    canWrite = true,
                    fk_FileId = file.pk_FileId,
                    SecurityObjectId = createdBy.SecurityObjectId
                };
                context.FileSecurities.Add(fileSecurity);
                context.SaveChanges();

                foreach (FolderSecurity security in folder.FolderSecurities)
                {
                    fileSecurity = context.FileSecurities.FirstOrDefault(d => d.SecurityObjectId == security.SecurityObjectId && d.fk_FileId == file.pk_FileId);
                    if (fileSecurity == null)
                    {
                        fileSecurity = new FileSecurity
                        {
                            CanDelete = security.canDelete,
                            canRead = security.canListObjects,
                            canWrite = security.canCreateFiles,
                            fk_FileId = file.pk_FileId,
                            SecurityObjectId = security.SecurityObjectId
                        };
                        context.FileSecurities.Add(fileSecurity);
                    }
                    else
                    {
                        fileSecurity.CanDelete = security.canDelete;
                        fileSecurity.canRead = security.canListObjects;
                        fileSecurity.canWrite = security.canCreateFiles;
                    }
                }


                context.SaveChanges();
                return file;
            }
        }

        /// <summary>
        ///     Set the permission on a file for the target User.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="user"></param>
        /// <param name="targetUser"></param>
        /// <param name="canRead"></param>
        /// <param name="canWrite"></param>
        /// <param name="canDelete"></param>
        /// <param name="context"></param>
        /// <param name="dbcxtransaction"></param>
        public static void SetPermissions(Guid fileId, Principal user, Principal targetUser, bool canRead, bool canWrite, bool canDelete, OnlineFilesEntities context = null, DbContextTransaction dbcxtransaction = null)
        {
            bool createdContext = false;
            bool createdTransaction = false;
            bool didRollback = false;
            if (context == null)
            {
                createdContext = true;
                context = new OnlineFilesEntities();
            }


            if (dbcxtransaction == null)
            {
                dbcxtransaction = context.Database.BeginTransaction();
                createdTransaction = true;
            }
            try
            {
                File targetfile = context.Files
                    .Include(d => d.FileSecurities)
                    .FirstOrDefault(d => d.pk_FileId == fileId);
                if (targetfile == null)
                    throw new Exception("File does not exist.");

                Folder target = context.Folders.Include(d => d.FolderSecurities).FirstOrDefault(d => d.pk_FolderId == targetfile.fk_FolderId);
                if (target == null)
                    throw new Exception("Parent Folder does not exist.");


                //Can the user Change Permissions
                if (target.FolderSecurities.Any(d => d.canChangePermissions && user.UserProfile.mySecurityGroups.Contains(d.SecurityObjectId)))
                {
                    var secRecord = targetfile.FileSecurities.FirstOrDefault(d => d.SecurityObjectId == targetUser.UserProfile.SecurityObjectId);
                    if (secRecord == null)
                    {
                        secRecord = new FileSecurity
                        {
                            fk_FileId = targetfile.pk_FileId,
                            CanDelete = canDelete,
                            canRead = canRead,
                            canWrite = canWrite,
                            SecurityObjectId = targetUser.UserProfile.SecurityObjectId
                        };
                        targetfile.FileSecurities.Add(secRecord);
                    }
                    else
                    {
                        secRecord.canRead = canRead;
                        secRecord.CanDelete = canDelete;
                        secRecord.canWrite = canWrite;
                    }
                    context.SaveChanges();
                }
                else
                {
                    throw new SecurityException("Not Authorized.");
                }
            }
            catch (Exception)
            {
                if (!createdTransaction)
                    throw;
                didRollback = true;
                dbcxtransaction.Rollback();
                throw;
            }

            finally
            {
                if (createdTransaction)
                {
                    if (!didRollback)
                        dbcxtransaction.Commit();
                    dbcxtransaction.Dispose();
                }
                if (createdContext)
                    context.Dispose();
            }
        }

        public static File Rename(Guid? fileId, Guid? folderId, string name, SecurityObject createdBy)
        {
            using (var context = new OnlineFilesEntities())
            {
                if (folderId == null)
                    throw new Exception("Bad Guid.");

                var folder = context.Folders
                    .Include(x => x.FolderSecurities)
                    .FirstOrDefault(d => d.pk_FolderId == folderId);
                if (folder == null)
                    throw new Exception("Folder Not Found.");

                if (!folder.FolderSecurities.Any(x => createdBy.mySecurityGroups.Contains(x.SecurityObjectId) && x.canCreateFiles))
                    throw new SecurityException("No Access.");

                var filechk = context.Files
                    .Include(x => x.FileSecurities)
                    .FirstOrDefault(d => d.Name == name);
                if (filechk != null)
                    throw new Exception("File Name already used.");
                var file = context.Files
                    .Include(x => x.FileSecurities)
                    .FirstOrDefault(d => d.pk_FileId == fileId);
                if (file != null)
                    file.Name = name;

                context.Files.AddOrUpdate(file);
                context.SaveChanges();

                return file;
            }
        }
    }
}