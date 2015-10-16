using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using WebDAVSharp.Data;
using WebDAVSharp.Data.Security;
using WebDAVSharp.Server.Exceptions;
using WebDAVSharp.Server.Stores;
using WebDAVSharp.Server.Stores.Locks;
using WebDAVSharp.Server.Stores.Locks.Enums;
using WebDAVSharp.Server.Stores.Locks.Interfaces;

namespace WebDAVSharp.SQL.SQLStore
{
    /// <summary>
    ///     This class provides the locking functionality.
    /// </summary>
    public class WebDavSqlStoreItemLock : WebDavStoreItemLockBase
    {
        /// <summary>
        ///     Retrieves all locks for the passed object
        /// </summary>
        /// <param name="storeItem">Item to look for locks for.</param>
        /// <returns></returns>
        public override List<IWebDavStoreItemLockInstance> GetLocks(IWebDavStoreItem storeItem)
        {
            try
            {
                List<IWebDavStoreItemLockInstance> items = new List<IWebDavStoreItemLockInstance>();
                using (var context = new OnlineFilesEntities())
                {
                    List<ObjectLockInfo> result;
                    WebDavSqlStoreCollection item = storeItem as WebDavSqlStoreCollection;
                    if (item != null)
                    {
                        var collection = item;
                        result = context.ObjectLockInfoes.AsNoTracking().Where(d => d.ObjectGuid == collection.ObjectGuid && d.isFolder).ToList();
                    }
                    else
                    {
                        WebDavSqlStoreDocument storeDocument = storeItem as WebDavSqlStoreDocument;
                        if (storeDocument != null)
                        {
                            var document = storeDocument;
                            result = context.ObjectLockInfoes.AsNoTracking().Where(d => d.ObjectGuid == document.ObjectGuid && !d.isFolder).ToList();
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                    items.AddRange(result.Select(lockInfo => new WebDavSqlStoreItemLockInstance(
                        PrincipleFactory.Instance.GetPrinciple(FromType.WebDav).UserProfile,
                        lockInfo.Path,
                        (WebDavLockScope) lockInfo.LockScope,
                        (WebDavLockType) lockInfo.LockType,
                        lockInfo.Owner.ToString(),
                        lockInfo.RequestedLockTimeout,
                        lockInfo.Token,
                        new XmlDocument(),
                        lockInfo.Depth,
                        this,
                        lockInfo.CreateDt
                        )));
                    return items;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Removes any expired locks
        /// </summary>
        public void CleanLocks()
        {
            try
            {
                using (var context = new OnlineFilesEntities())
                {
                    context.ObjectLockInfoes.RemoveRange(context.ObjectLockInfoes.Where(d => d.ExpirationDate < DateTime.Now).ToList());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Locks the object passed.
        /// </summary>
        /// <param name="storeItem"></param>
        /// <param name="lockscope"></param>
        /// <param name="locktype"></param>
        /// <param name="lockowner"></param>
        /// <param name="requestedlocktimeout"></param>
        /// <param name="locktoken"></param>
        /// <param name="requestDocument"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public override int Lock(IWebDavStoreItem storeItem, WebDavLockScope lockscope, WebDavLockType locktype,
            string lockowner, double? requestedlocktimeout, out Guid? locktoken, XmlDocument requestDocument, int depth)
        {
            locktoken = null;
            try
            {
                var sqlStoreItem = (WebDavSqlStoreItem) storeItem;


                CleanLocks();


                using (OnlineFilesEntities context = new OnlineFilesEntities())
                {
                    WebDavSqlStoreItemLockInstance inst;
                    if (!context.ObjectLockInfoes.Any(d => d.ObjectGuid == sqlStoreItem.ObjectGuid && d.isFolder == sqlStoreItem.IsCollection))
                    {
                        inst = new WebDavSqlStoreItemLockInstance(
                            PrincipleFactory.Instance.GetPrinciple(FromType.WebDav).UserProfile,
                            storeItem.ItemPath,
                            lockscope, locktype, lockowner,
                            requestedlocktimeout,
                            null, requestDocument, depth, this);

                        locktoken = CreateSqlLock(ref inst, sqlStoreItem);

                        return (int) HttpStatusCode.OK;
                    }
                    switch (lockscope)
                    {
                        case WebDavLockScope.Exclusive:
//#if DEBUG
//                            WebDavServer.Log.Debug("Lock Creation Failed (Exclusive), URI already has a lock.");
//#endif

                            return 423;
                        case WebDavLockScope.Shared:
                            if (context.ObjectLockInfoes.Any(d =>
                                d.ObjectGuid == sqlStoreItem.ObjectGuid &&
                                d.isFolder == sqlStoreItem.IsCollection &&
                                d.LockScope == (int) WebDavLockScope.Exclusive))
                            {
//#if DEBUG
//                                WebDavServer.Log.Debug("Lock Creation Failed (Shared), URI has exclusive lock.");
//#endif
                                return 423;
                            }
                            break;
                    }

                    //If the scope is shared and all other locks on this uri are shared we are ok, otherwise we fail.
                    //423 (Locked), potentially with 'no-conflicting-lock' precondition code - 
                    //There is already a lock on the resource that is not compatible with the 
                    //requested lock (see lock compatibility table above).

                    //If it gets to here, then we are most likely creating another shared lock on the file.
                    inst = new WebDavSqlStoreItemLockInstance(
                        PrincipleFactory.Instance.GetPrinciple(FromType.WebDav).UserProfile,
                        storeItem.ItemPath, lockscope, locktype, lockowner,
                        requestedlocktimeout, null, requestDocument, depth, this);

                    locktoken = CreateSqlLock(ref inst, sqlStoreItem);
                    return (int) HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Creates a new SQL Lock.
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="storeItem"></param>
        /// <returns></returns>
        private static Guid CreateSqlLock(ref WebDavSqlStoreItemLockInstance inst, WebDavSqlStoreItem storeItem)
        {
            try
            {
                using (var context = new OnlineFilesEntities())
                {
                    var t = context.Files.FirstOrDefault(d => d.pk_FileId == storeItem.ObjectGuid);


                    ObjectLockInfo info = new ObjectLockInfo
                    {
                        CreateDt = inst.CreateDate,
                        Depth = inst.Depth,
                        ExpirationDate = inst.ExpirationDate,
                        LockScope = (int) inst.LockScope,
                        LockType = (int) inst.LockType,
                        ObjectGuid = (Guid) storeItem.ObjectGuid,
                        OwnerId = inst.SoOwner.SecurityObjectId,
                        Path = storeItem.Href.ToString(),
                        RequestedLockTimeout = inst.RequestedLockTimeout,
                        isFolder = storeItem.IsCollection
                    };

                    context.ObjectLockInfoes.Add(info);
                    context.SaveChanges();
                    inst.Token = info.Token;
                    return info.Token;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     Refreshes an existing lock
        /// </summary>
        /// <param name="storeItem"></param>
        /// <param name="locktoken"></param>
        /// <param name="requestedlocktimeout"></param>
        /// <param name="requestDocument"></param>
        /// <returns></returns>
        public override int RefreshLock(IWebDavStoreItem storeItem, Guid? locktoken, double? requestedlocktimeout, out XmlDocument requestDocument)
        {
            try
            {
                CleanLocks();
                requestDocument = null;
                if (locktoken == null)
                    throw new WebDavNotFoundException("Must have a lock to refresh.");

                using (var context = new OnlineFilesEntities())
                {
                    var info = context.ObjectLockInfoes.FirstOrDefault(d => d.Token == locktoken);
                    if (info == null)
                        throw new WebDavNotFoundException("Lock Token Not Found.");

                    IWebDavStoreItemLockInstance inst = new WebDavSqlStoreItemLockInstance(
                        PrincipleFactory.Instance.GetPrinciple(FromType.WebDav).UserProfile,
                        info.Path, (WebDavLockScope) info.LockScope, (WebDavLockType) info.LockType,
                        info.Owner.ToString(),
                        info.RequestedLockTimeout, info.Token, new XmlDocument(), info.Depth, this, info.CreateDt
                        );

                    inst.RefreshLock(requestedlocktimeout);
                    info.CreateDt = inst.CreateDate;
                    info.ExpirationDate = inst.ExpirationDate;
                    info.RequestedLockTimeout = inst.RequestedLockTimeout;
                    context.SaveChanges();
                    requestDocument = new XmlDocument();
                    return (int) HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        ///     unlocks an existing lock
        /// </summary>
        /// <param name="storeItem"></param>
        /// <param name="locktoken"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public override int UnLock(IWebDavStoreItem storeItem, Guid? locktoken, string owner)
        {
            try
            {
                CleanLocks();
                if (locktoken == null)
                {
//#if DEBUG
//                    WebDavServer.Log.Debug("Unlock failed, No Token!.");
//#endif
                    return (int) HttpStatusCode.BadRequest;
                }
                using (var context = new OnlineFilesEntities())
                {
                    var info = context.ObjectLockInfoes.FirstOrDefault(d => d.Token == locktoken);

                    if (info == null)
                        throw new WebDavNotFoundException("Lock Token Not Found.");

                    if (info.OwnerId != PrincipleFactory.Instance.GetPrinciple(FromType.WebDav).UserProfile.SecurityObjectId)
                        throw new WebDavUnauthorizedException("You did not create the lock.");


                    context.ObjectLockInfoes.Remove(info);
                    context.SaveChanges();
                    return (int) HttpStatusCode.NoContent;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                throw;
            }
        }
    }
}