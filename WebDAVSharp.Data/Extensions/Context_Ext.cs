﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Security;
using WebDAVSharp.Data.Security;

namespace WebDAVSharp.Data
{


    public partial class OnlineFiles_CatalogEntities : DbContext
    {
    }

    public static class ContextHelper
    {
        /// <summary>
        /// Object Type.
        /// </summary>
        public enum fsObjectType
        {
            File,
            Folder
        }

        /// <summary>
        /// Purges the System of all information regarding this Object and it's children.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        public static void HardDelete( fsObjectType type, Guid id)
        {
            
            if (type == fsObjectType.File)
                HardDeleteFile(null, id);
            else
                HardDeleteFolder(null, id);
        }

        /// <summary>
        /// Purges the Folder and all it's children from the system
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folderId"></param>
        private static void HardDeleteFolder(this OnlineFilesEntities context, Guid folderId)
        {



            bool createdContext = false;
            if (context == null)
            {
                createdContext = true;
                context = new OnlineFilesEntities();
            }
            try
            {
                //Little bit of recursion here
                var children = context.Folders.Where(x => x.fk_ParentFolderId == folderId).Select(c => c.pk_FolderId).ToList();
                foreach (var f in children)
                    context.HardDeleteFolder(f);



                Folder folder = context.Folders
                    .Include(x => x.FolderSecurities)
                    .Include(x => x.Files)
                    .FirstOrDefault(d => d.pk_FolderId == folderId);

                if (folder == null)
                    throw new FileNotFoundException("File Not found with: " + folderId);
                
                //Delete all the files.
                foreach (File file in folder.Files.ToList())
                    context.HardDeleteFile(file.pk_FileId);

                context.FolderSecurities.RemoveRange(folder.FolderSecurities);
                context.Folders.Remove(folder);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                if (createdContext)
                {
                    context.SaveChanges();
                    context.Dispose();
                    context = null;
                }
            }
        }

        /// <summary>
        /// Purges the file and all it's data from the system.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileId"></param>
        private static void HardDeleteFile(this OnlineFilesEntities context, Guid fileId)
        {
            bool createdContext = false;
            if (context == null)
            {
                createdContext = true;
                context = new OnlineFilesEntities();
            }
            try
            {

                File file = context.Files
                    .Include(c => c.FileSecurities)
                    .Include(c => c.FileDatas.Select(d => d.Catalog))
                    .FirstOrDefault(x => x.pk_FileId == fileId);
                if (file == null)
                    throw new FileNotFoundException("File Not found with: " + fileId);

                //Remove the data from the catalog.
                foreach (FileData data in file.FileDatas)
                    using (var cfd = new OnlineFiles_CatalogEntities(data.Catalog.EntityConnectionString))
                    {
                        cfd.FileCatalogEntries.Remove(cfd.FileCatalogEntries.FirstOrDefault(d => d.pk_FileCatalogEntryId == data.fk_ContentId));
                        cfd.SaveChanges();
                    }

                //Remove Pointer records to data
                context.FileDatas.RemoveRange(file.FileDatas);
                //Remove File Security Records.
                context.FileSecurities.RemoveRange(file.FileSecurities);

                context.Files.Remove(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                if (createdContext)
                {
                    context.SaveChanges();
                    context.Dispose();
                    context = null;
                }
            }
        }

        /// <summary>
        /// Retrieves all the Folders in the Trash Bin
        /// </summary>
        /// <param name="context"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static List<Folder> GetTrashBinFolder(this OnlineFilesEntities context, Principal p)
        {
            List<Folder> found;
            bool createdContext = false;
            if (context == null)
            {
                createdContext = true;
                context = new OnlineFilesEntities();
            }
            try
            {

                found = context.Folders.Where(d => d.IsDeleted && d.OwnerId == p.UserProfile.SecurityObjectId)
                .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                if (createdContext)
                {
                    context.Dispose();
                    context = null;
                }
            }
            return found;
        }

        /// <summary>
        /// Retrieves all the Child Folders for the folder
        /// </summary>
        /// <param name="context"></param>
        /// <param name="Key"></param>
        /// <param name="p"></param>
        /// <param name="ReadOnly"></param>
        /// <returns></returns>
        public static List<Folder> GetChildFolders(this OnlineFilesEntities context, Guid? Key, Principal p, bool ReadOnly = false)
        {
            List<Folder> found = new List<Folder>();
            bool createdContext = false;
            if (context == null)
            {
                createdContext = true;
                context = new OnlineFilesEntities();
            }
            try
            {
                DbQuery<Folder> source = ReadOnly ? context.Folders.AsNoTracking() : context.Folders;

                if (Key == new Guid())
                {
                    found = source.Where(d => d.fk_ParentFolderId == Key && !(d.IsDeleted) &&
                          (d.OwnerId == p.UserProfile.SecurityObjectId)).ToList();
                }
                else
                {

                    found = source.Where(d => d.fk_ParentFolderId == Key && !(d.IsDeleted) &&
                                              (d.OwnerId == p.UserProfile.SecurityObjectId ||
                                               d.FolderSecurities.Any(x => x.canListObjects && p.UserProfile.mySecurityGroups.Contains(x.SecurityObjectId))
                                                  )).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                if (createdContext)
                {
                    context.Dispose();
                    context = null;
                }
            }
            return found;

        }

           



        /// <summary>
        /// Retrieves all the child folders for the file.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folder"></param>
        /// <param name="p"></param>
        /// <param name="ReadOnly"></param>
        /// <returns></returns>
        public static List<Folder> GetChildFolders(this OnlineFilesEntities context, Folder folder, Principal p, bool ReadOnly = false)
        {
            return GetChildFolders(context, folder.pk_FolderId, p, ReadOnly);

        }

        /// <summary>
        ///     Gets all the files in the trash bin.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static List<File> GetTrashBinFile(this OnlineFilesEntities context, Principal p)
        {
            List<File> found;
            bool createdContext = false;
            if (context == null)
            {
                createdContext = true;
                context = new OnlineFilesEntities();
            }
            try
            {
                found = context.Files.Where(d => d.IsDeleted && d.OwnerId == p.UserProfile.SecurityObjectId)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                if (createdContext)
                {
                    context.Dispose();
                    context = null;
                }
            }
            return found;
        }

        /// <summary>
        ///     Gets all the files in a folder.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folderkey"></param>
        /// <param name="p"></param>
        /// <param name="readOnly"></param>
        /// <returns></returns>
        public static List<File> GetChildFiles(this OnlineFilesEntities context, Guid? folderkey, Principal p, bool readOnly = false)
        {
            List<File> found;
            bool createdContext = false;
            if (context == null)
            {
                createdContext = true;
                context = new OnlineFilesEntities();
            }
            try
            {
                var folder = context.Folders.Include(x => x.FolderSecurities).FirstOrDefault(d => d.pk_FolderId == folderkey);
                if (folder == null)
                    throw new SecurityException("No Access");
                if (!folder.FolderSecurities.Any(x => p.UserProfile.mySecurityGroups.Contains(x.SecurityObjectId) && x.canListObjects))
                    throw new SecurityException("No Access");


                DbQuery<File> source = readOnly ? context.Files.AsNoTracking() : context.Files;

                found = source.Where(d => d.fk_FolderId == folderkey && !(d.IsDeleted) && (d.OwnerId == p.UserProfile.SecurityObjectId ||
                                                                                           d.FileSecurities.Any(x => x.canRead && p.UserProfile.mySecurityGroups.Contains(x.SecurityObjectId))))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                if (createdContext)
                    context.Dispose();
            }
            return found;
        }

        /// <summary>
        ///     Gets all the files in a folder
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folder"></param>
        /// <param name="p"></param>
        /// <param name="readOnly"></param>
        /// <returns></returns>
        public static List<File> GetChildFiles(this OnlineFilesEntities context, Folder folder, Principal p, bool readOnly = false)
        {
            return GetChildFiles(context, folder.pk_FolderId, p, readOnly);
        }
    }
}
