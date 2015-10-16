using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using WebDAVSharp.Data.HelperClasses;
using WebDAVSharp.Server;

namespace WebDAVSharp.Data.Security
{
    public class PrincipleFactory : CacheBase
    {
        private static PrincipleFactory _instance;

        private PrincipleFactory()
        {
        }

        public static PrincipleFactory Instance => _instance ?? (_instance = new PrincipleFactory());

        public Principal GetPrinciple(string username)
        {
            string user = Principal.GetLogin(username);
            Principal p = (Principal) GetCachedObject(user);
            if (p != null) return p;
            p = new Principal(user);
            AddCacheObject(user, p, TimeSpan.FromMinutes(60));
            return p;
        }

        public Principal GetPrinciple(HttpContext current)
        {
            string user = Principal.GetLogin(current.User.Identity.Name);
            user = user == string.Empty ? Principal.GetLogin(Environment.UserName) : user;
            Principal p = (Principal) GetCachedObject(user);
            if (p != null) return p;
            p = new Principal(user);
            AddCacheObject(user, p, TimeSpan.FromMinutes(60));
            return p;
        }

        public Principal GetPrinciple(FromType fromType)
        {
            string user;
            switch (fromType)
            {
                case FromType.Web:

                    user = Principal.GetLogin(HttpContext.Current.User.Identity.Name);
                    user = user == string.Empty ? Principal.GetLogin(Environment.UserName) : user;
                    break;
                case FromType.WebDav:
                    user = ((IIdentity) Thread.GetData(Thread.GetNamedDataSlot(WebDavServer.HttpUser))).Name;
                    break;
                default:
                    throw new Exception("No Enum Set");
            }
            Principal p = (Principal) GetCachedObject(user);
            if (p != null) return p;
            p = new Principal(user);
            AddCacheObject(user, p, TimeSpan.FromMinutes(60));
            return p;
        }

        public Principal GetPrinciple()
        {
            string user = Principal.GetLogin(HttpContext.Current.User.Identity.Name);
            user = user == string.Empty ? Principal.GetLogin(Environment.UserName) : user;
            Principal p = (Principal) GetCachedObject(user);
            if (p != null)
                return p;
            p = new Principal(user);
            AddCacheObject(user, p, TimeSpan.FromMinutes(60));
            return p;
        }
    }
}