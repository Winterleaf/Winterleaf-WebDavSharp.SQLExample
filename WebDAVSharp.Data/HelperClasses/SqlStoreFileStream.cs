using System;
using System.IO;
using System.Linq;

namespace WebDAVSharp.Data.HelperClasses
{
    public class SqlStoreFileStream : MemoryStream
    {
        public Guid CatalogId { get; set; }
        public Guid FileDataId { get; set; }
        public Guid FileCatalogEntryId { get; set; }
        public string Path { get; set; }
        public dynamic WebDavSqlStoreDocumentFactoryInstance { get; set; } = null;

        public override void Close()
        {
            using (var context = new OnlineFilesEntities())
            {
                Catalog catalog = context.Catalogs.FirstOrDefault(d => d.pk_CatalogId == CatalogId);
                if (catalog == null)
                    throw new Exception("No Catalog.");

                using (var ctx = new OnlineFiles_CatalogEntities(catalog.EntityConnectionString))
                {
                    FileCatalogEntry entry = ctx.FileCatalogEntries.FirstOrDefault(d => d.pk_FileCatalogEntryId == FileCatalogEntryId);
                    if (entry == null)
                        throw new Exception("Catalog Entry is null.");
                    entry.binaryData = ToArray();
                    ctx.SaveChanges();
                }

                FileData file = context.FileDatas.FirstOrDefault(d => d.pk_FileDataId == FileDataId);
                if (file == null)
                    throw new Exception("File is null.");
                file.Size = ToArray().Count();
                context.SaveChanges();

                if (WebDavSqlStoreDocumentFactoryInstance != null)
                {
                    WebDavSqlStoreDocumentFactoryInstance.InvalidateDocumentPath(Path);
                }
            }
            base.Close();
        }
    }
}