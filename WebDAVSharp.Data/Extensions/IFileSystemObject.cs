using System;
using WebDAVSharp.Data.HelperClasses;

namespace WebDAVSharp.Data.Extensions
{
    public enum FileSystemObjectType
    {
        File,
        Folder
    }

    public interface IFileSystemObject
    {
        #region Folder Properties

        /// <summary>
        ///     Catalog containing file data
        /// </summary>
        Guid? FolderCatalogCollectionId { get; }

        #endregion

        #region All Objects

        /// <summary>
        ///     The Dos File Information
        /// </summary>
        SqlStoreFileInfo DosFileInfo { get; }

        /// <summary>
        ///     Object Type, File or Folder
        /// </summary>
        FileSystemObjectType ObjectType { get; }

        long FileSize { get; }

        /// <summary>
        ///     Parent Folder ID
        /// </summary>
        Guid ParentObject { get; }

        /// <summary>
        ///     Either the file or folder id
        /// </summary>
        Guid FileSystemId { get; }

        /// <summary>
        ///     The display name of the object
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        ///     Date created
        /// </summary>
        DateTime CreatedDate { get; }

        /// <summary>
        ///     Date Deleted
        /// </summary>
        DateTime? DeletedDate { get; }

        /// <summary>
        ///     Object is deleted or not
        /// </summary>
        bool ObjectIsDeleted { get; }

        /// <summary>
        ///     Who Deleted
        /// </summary>
        SecurityObject DeletedBySecurityObject { get; }

        /// <summary>
        ///     Who Owns object
        /// </summary>
        SecurityObject OwnerSecurityObject { get; }

        /// <summary>
        ///     Who Created.
        /// </summary>
        SecurityObject CreatedBySecurityObject { get; }

        #endregion

        #region FileProperties

        /// <summary>
        ///     Mime type information
        /// </summary>
        string FileMimeType { get; }

        /// <summary>
        ///     File is revisioned
        /// </summary>
        bool? FileIsRevisioned { get; }

        #endregion
    }
}