using System;
using System.IO;
using System.Linq;
using WebDAVSharp.Data.Security;
using WebDAVSharp.Server.Stores;
using WebDAVSharp.Server.Stores.BaseClasses;

namespace WebDAVSharp.Data.HelperClasses
{
    public class SqlStoreFileInfo : WebDavFileInfoBase
    {
        public Guid? ObjectGuid { get; set; }
        public IWebDavStoreCollection Parent { get; set; }

        public override void Apply()
        {
            using (var context = new OnlineFilesEntities())
            {
                if (Directory)
                {
                    Folder folder = context.Folders.FirstOrDefault(d => d.pk_FolderId == ObjectGuid);
                    if (folder == null)
                        return;

                    folder.SetWin32Attribute(FileAttributes.Directory, Directory);
                    folder.SetWin32Attribute(FileAttributes.Archive, Archive);
                    folder.SetWin32Attribute(FileAttributes.Compressed, Compressed);
                    folder.SetWin32Attribute(FileAttributes.Device, Device);
                    folder.SetWin32Attribute(FileAttributes.Encrypted, Encrypted);
                    folder.SetWin32Attribute(FileAttributes.Hidden, Hidden);
                    folder.SetWin32Attribute(FileAttributes.IntegrityStream, IntegrityStream);
                    folder.SetWin32Attribute(FileAttributes.Normal, Normal);
                    folder.SetWin32Attribute(FileAttributes.NoScrubData, NoScrubData);
                    folder.SetWin32Attribute(FileAttributes.NotContentIndexed, NotContentIndexed);
                    folder.SetWin32Attribute(FileAttributes.Offline, Offline);
                    folder.SetWin32Attribute(FileAttributes.ReadOnly, ReadOnly);
                    folder.SetWin32Attribute(FileAttributes.ReparsePoint, ReparsePoint);
                    folder.SetWin32Attribute(FileAttributes.SparseFile, SparseFile);
                    folder.SetWin32Attribute(FileAttributes.System, System);
                    folder.SetWin32Attribute(FileAttributes.Temporary, Temporary);
                    context.SaveChanges();
                }
                else
                {
                    File file = context.Files.FirstOrDefault(d => d.pk_FileId == ObjectGuid);
                    if (file == null)
                        return;
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.Directory, Directory);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.Archive, Archive);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.Compressed, Compressed);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.Device, Device);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.Encrypted, Encrypted);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.Hidden, Hidden);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.IntegrityStream, IntegrityStream);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.Normal, Normal);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.NoScrubData, NoScrubData);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.NotContentIndexed, NotContentIndexed);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.Offline, Offline);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.ReadOnly, ReadOnly);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.ReparsePoint, ReparsePoint);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.SparseFile, SparseFile);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.System, System);
                    file.SetWin32Attribute(PrincipleFactory.Instance.GetPrinciple(FromType.WebDav), FileAttributes.Temporary, Temporary);
                    context.SaveChanges();
                }
            }
        }
    }
}