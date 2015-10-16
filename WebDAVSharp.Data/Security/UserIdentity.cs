// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserIdentity.cs" company="B. F. Saul Co.">
//   © 2015 B. F. Saul Co.
// </copyright>
// <summary>
//   The user identity.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace WebDAVSharp.Data.Security
{
    /// <summary>
    ///     The user identity.
    /// </summary>
    [Serializable]
    public class UserIdentity : IdentityBase<UserIdentity>
    {
        #region Public Properties

        /// <summary>
        ///     Gets the user profile.
        /// </summary>
        public SecurityObject UserProfile
        {
            get; private set;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The get identity.
        /// </summary>
        /// <param name="username">
        ///     The username.
        /// </param>
        /// <returns>
        ///     The <see cref="UserIdentity" />.
        /// </returns>
        internal static UserIdentity GetIdentity(string username)
        {
            using (var context = new OnlineFilesEntities())
            {
                username = Principal.GetLogin(username);
                var userIdentity = new UserIdentity();

                // Get the SecurityObject being requested.
                SecurityObject userProfile = context.GetUserValidation(username, ConfigurationManager.AppSettings["LDAP_SETTING"]).FirstOrDefault();
                if (userProfile == null)
                    throw new Exception("User Not Found");
                userProfile._MySecurityGroups = context.GetSecurityTokens(userProfile.SecurityObjectId).ToList();
                context.SaveChanges();
                if (userProfile.HomeFolder == null)
                {
                    Folder homeFolder = Folder.Create(userProfile.FullName, new Guid(), userProfile, true, true);
                    userProfile.HomeFolder = homeFolder.pk_FolderId;
                    context.SaveChanges();
                }
                // Ensure that the SecurityObject's Permissions are loaded.
                context.Entry(userProfile).Collection(so => so.SecurityObjectPermissions).Query().Include(sop => sop.Permission).Load();
                userIdentity.LoadUser(userProfile);
                return userIdentity;
            }
        }


        /// <summary>
        ///     The load user.
        /// </summary>
        /// <param name="data">
        ///     The data.
        /// </param>
        private void LoadUser(SecurityObject data)
        {
            if (data != null)
            {
                UserId = data.SecurityObjectId;
                Name = data.Username;
                IsAuthenticated = true;
                AuthenticationType = "Membership";
                Permissions.AddRange(data.SecurityObjectPermissions.Select(sop => sop.Permission).ToList());
                UserProfile = data;
            }
            else
            {
                Name = string.Empty;
                IsAuthenticated = false;
                AuthenticationType = string.Empty;
                Permissions = new List<Permission>();
                UserProfile = new SecurityObject();
            }
        }

        #endregion
    }
}