// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebDavSqlStoreDocumentFactory.cs" company="">
//   
// </copyright>
// <summary>
//   The web dav sql store document factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using WebDAVSharp.Data.HelperClasses;
using WebDAVSharp.Data.Security;
using WebDAVSharp.Server.Stores;

#if DEBUG
using Common.Logging;

#endif

namespace WebDAVSharp.SQL.SQLStore
{
    /// <summary>
    ///     The web dav sql store document factory.
    /// </summary>
    internal class WebDavSqlStoreDocumentFactory : CacheBase
    {
#if DEBUG

    /// <summary>
    ///     The log.
    /// </summary>
        private static readonly ILog Log = LogManager.GetLogger< WebDavSqlStoreDocumentFactory>();
#endif

        /// <summary>
        ///     The _instance.
        /// </summary>
        private static WebDavSqlStoreDocumentFactory _instance;

        /// <summary>
        ///     Prevents a default instance of the <see cref="WebDavSqlStoreDocumentFactory" /> class from being created.
        /// </summary>
        private WebDavSqlStoreDocumentFactory()
        {
        }

        /// <summary>
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets the store.
        /// </summary>
        public IWebDavStore Store { get; set; }

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        public static WebDavSqlStoreDocumentFactory Instance => _instance ?? (_instance = new WebDavSqlStoreDocumentFactory());

        /// <summary>
        ///     The invalidate document path.
        /// </summary>
        /// <param name="path">
        ///     The path.
        /// </param>
        public void InvalidateDocumentPath(string path)
        {
#if DEBUG
            Log.Info("WebDavSqlStoreDocument Invalidating " + path);
#endif
            RemoveCacheObject(path);
            Store.RemoveCacheObject(path);
        }

        /// <summary>
        ///     The get document.
        /// </summary>
        /// <param name="parentCollection">
        ///     The parent collection.
        /// </param>
        /// <param name="path">
        ///     The path.
        /// </param>
        /// <param name="rootPath">
        ///     The root path.
        /// </param>
        /// <param name="rootGuid">
        ///     The root guid.
        /// </param>
        /// <returns>
        ///     The <see cref="WebDavSqlStoreDocument" />.
        /// </returns>
        public WebDavSqlStoreDocument GetDocument(IWebDavStoreCollection parentCollection, string path, string rootPath, Guid rootGuid)
        {
            if (!Enabled)
                return new WebDavSqlStoreDocument(parentCollection, path, rootPath, rootGuid, Store);

            var p = PrincipleFactory.Instance.GetPrinciple(FromType.WebDav);
            string userkey = p.UserProfile.SecurityObjectId.ToString();
            CacheBase mc = GetCachedObject(path) as CacheBase;
            WebDavSqlStoreDocument itm = null;
            if (mc != null)
                itm = mc.GetCachedObject(userkey) as WebDavSqlStoreDocument;

            if (itm != null)
                return itm;

            itm = new WebDavSqlStoreDocument(parentCollection, path, rootPath, rootGuid, Store);
            if (mc == null)
            {
                mc = new CacheBase();
                mc.AddCacheObject(userkey, itm);
                AddCacheObject(path, mc);
            }
            else
                mc.AddCacheObject(userkey, itm);
            return itm;
        }
    }
}