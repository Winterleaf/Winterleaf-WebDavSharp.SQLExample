using System;
using System.Xml;
using WebDAVSharp.Data;
using WebDAVSharp.Server.Stores.Locks;
using WebDAVSharp.Server.Stores.Locks.Enums;
using WebDAVSharp.Server.Stores.Locks.Interfaces;

namespace WebDAVSharp.SQL.SQLStore
{
    internal class WebDavSqlStoreItemLockInstance : WebDavStoreItemLockInstance
    {
        public WebDavSqlStoreItemLockInstance(SecurityObject so, string path, WebDavLockScope lockscope, WebDavLockType locktype, string owner, double? requestedlocktimeout, Guid? token, XmlDocument requestdocument, int depth, IWebDavStoreItemLock lockSystem, DateTime? createdate = null)
            : base(path, lockscope, locktype, owner, requestedlocktimeout, token, requestdocument, depth, lockSystem, createdate)
        {
            SoOwner = so;
        }

        public SecurityObject SoOwner { get; set; }
    }
}