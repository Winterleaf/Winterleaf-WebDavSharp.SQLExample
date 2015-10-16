#if DEBUG
using WebDAVSharp.Server.Utilities;
using Common.Logging;
#endif
using System;
using WebDAVSharp.Data.HelperClasses;
using WebDAVSharp.Data.Security;
using WebDAVSharp.Server.Stores;

namespace WebDAVSharp.SQL.SQLStore
{
    public class WebDavSqlStoreCollectionFactory : CacheBase
    {
#if DEBUG
        private static readonly ILog Log = LogManager.GetLogger< WebDavSqlStoreCollectionFactory>();
#endif
        private static WebDavSqlStoreCollectionFactory _instance;

        private WebDavSqlStoreCollectionFactory()
        {
        }

        /// <summary>
        /// </summary>
        public bool Enabled { get; set; }

        public IWebDavStore Store { get; set; }
        public static WebDavSqlStoreCollectionFactory Instance => _instance ?? (_instance = new WebDavSqlStoreCollectionFactory());

        public void InvalidateCollection(string path)
        {
#if DEBUG
            Log.Info("WebDavSqlStoreCollection Invalidating " + path);
#endif
            RemoveCacheObject(path);
            Store.RemoveCacheObject(path);
        }

        public WebDavSqlStoreCollection GetCollection(IWebDavStoreCollection parentCollection, string path, String rootPath, Guid rootGuid)
        {
            var p = PrincipleFactory.Instance.GetPrinciple(FromType.WebDav);
            string userkey = p.UserProfile.SecurityObjectId.ToString();
            CacheBase mc = GetCachedObject(path) as CacheBase;
            WebDavSqlStoreCollection itm = null;
            if (mc != null)
                itm = mc.GetCachedObject(userkey) as WebDavSqlStoreCollection;
            if (itm != null)
                return itm;
            itm = new WebDavSqlStoreCollection(parentCollection, path, rootPath, rootGuid, Store);
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