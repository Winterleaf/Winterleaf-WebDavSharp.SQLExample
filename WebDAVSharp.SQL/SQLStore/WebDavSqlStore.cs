using System;
using System.Security.Principal;
using WebDAVSharp.Data.Security;
using WebDAVSharp.Server.Stores;
using WebDAVSharp.Server.Stores.BaseClasses;
using WebDAVSharp.Server.Stores.Locks.Interfaces;

namespace WebDAVSharp.SQL.SQLStore
{
    /// <summary>
    /// </summary>
    public class WebDavSqlStore : WebDavStoreBase, IWebDavStore
    {
        /// <summary>
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="rootGuid"></param>
        /// <param name="lockSystem"></param>
        public WebDavSqlStore(string rootPath, Guid rootGuid, IWebDavStoreItemLock lockSystem)
            : base(lockSystem)
        {
            RootPath = rootPath;
            WebDavSqlStoreCollectionFactory.Instance.Store = this;
            WebDavSqlStoreCollectionFactory.Instance.Enabled = true;
            WebDavSqlStoreDocumentFactory.Instance.Store = this;
            WebDavSqlStoreDocumentFactory.Instance.Enabled = true;
        }

        public string RootPath { get; set; }

        public Guid RootGuid => new Guid();

        /// <summary>
        /// </summary>
        public new IWebDavStoreCollection Root => WebDavSqlStoreCollectionFactory.Instance.GetCollection(null, RootPath, RootPath, RootGuid);

        public override void UserAuthenticated(IIdentity ident)
        {
            var so = PrincipleFactory.Instance.GetPrinciple(FromType.WebDav).UserProfile;
        }
    }
}