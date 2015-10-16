// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Principal.cs" company="B. F. Saul Co.">
//   © 2014 B. F. Saul Co.
// </copyright>
// <summary>
//   The principal.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using WebDAVSharp.Server;

namespace WebDAVSharp.Data.Security
{
    public static class Util
    {
    }

    /// <summary>
    ///     The principal.
    /// </summary>
    [Serializable]
    public class Principal //: CacheBase
    {
        /// <summary>
        ///     The identity.
        /// </summary>
        private IIdentity _identity;

        public IIdentity Identity
        {
            get { return _identity; }

            private set { _identity = value; }
        }

        public SecurityObject UserProfile => ((UserIdentity) Identity).UserProfile;
        public Guid? UserId => ((UserIdentity) _identity).UserId;

        /// <summary>
        ///     The new user.
        /// </summary>
        public event Action NewUser;

        internal static string GetLogin(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            int stop = s.IndexOf("\\", StringComparison.Ordinal);
            return (stop > -1) ? s.Substring(stop + 1, s.Length - stop - 1) : s;
        }

        /// <summary>
        ///     The has permission.
        /// </summary>
        /// <param name="permissionType">
        ///     The permission type.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool HasPermission(PermissionType permissionType)
        {
            var check = _identity as ICheckPermissions;

            return check != null && check.HasPermission(permissionType);
        }

        public bool HasAnyPermission(PermissionType[] permissionType)
        {
            var check = _identity as ICheckPermissions;

            return check != null && permissionType.Any(check.HasPermission);
        }

        public bool CanSeeMenu()
        {
            var check = _identity as ICheckPermissions;

            return check != null && check.CanSeeMenu();
        }

        /// <summary>
        ///     The load.
        /// </summary>
        /// <param name="username">
        ///     The username.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        private bool Load(string username)
        {
            UserIdentity newIdentity = UserIdentity.GetIdentity(username);
            return SetPrincipal(newIdentity);
        }

        /// <summary>
        ///     The on new user.
        /// </summary>
        private void OnNewUser()
        {
            NewUser?.Invoke();
        }

        /// <summary>
        ///     The set principal.
        /// </summary>
        /// <param name="newIdentity">
        ///     The new identity.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        private bool SetPrincipal(IIdentity newIdentity)
        {
            if (newIdentity.IsAuthenticated)
            {
                Identity = newIdentity;
            }

            OnNewUser();
            return Identity.IsAuthenticated;
        }

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Principal" /> class.
        /// </summary>
        /// <param name="identity">
        ///     Identity object for the user.
        /// </param>
        internal Principal(IIdentity identity)
        {
            _identity = identity;
        }

        internal Principal(string username)
        {
            string user = GetLogin(username);
            user = user == string.Empty ? GetLogin(Environment.UserName) : user;
            Load(user);
        }

        internal Principal(HttpContext current)
        {
            string networkUsername = GetLogin(current.User.Identity.Name);
            networkUsername = networkUsername == string.Empty ? GetLogin(Environment.UserName) : networkUsername;
            Load(networkUsername);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Principal" /> class.
        /// </summary>
        internal Principal(FromType fromType)
        {
            string networkUsername;
            switch (fromType)
            {
                case FromType.Web:

                    networkUsername = GetLogin(HttpContext.Current.User.Identity.Name);
                    networkUsername = networkUsername == string.Empty ? GetLogin(Environment.UserName) : networkUsername;
                    Load(networkUsername);
                    break;
                case FromType.WebDav:
                    networkUsername = ((IIdentity) Thread.GetData(Thread.GetNamedDataSlot(WebDavServer.HttpUser))).Name;
                    Load(networkUsername);
                    break;
            }
        }

        internal Principal()
        {
            string networkUsername = GetLogin(HttpContext.Current.User.Identity.Name);
            networkUsername = networkUsername == string.Empty ? GetLogin(Environment.UserName) : networkUsername;
            Load(networkUsername);
        }

        #endregion
    }
}