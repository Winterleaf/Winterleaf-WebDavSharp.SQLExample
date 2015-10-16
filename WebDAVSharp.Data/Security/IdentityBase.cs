// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdentityBase.cs" company="B. F. Saul Co.">
//   © 2014 B. F. Saul Co.
// </copyright>
// <summary>
//   Provides a base class to simplify creation of
//   a .NET identity object for use with Principal.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace WebDAVSharp.Data.Security
{
    /// <summary>
    ///     Provides a base class to simplify creation of
    ///     a .NET identity object for use with Principal.
    /// </summary>
    /// <typeparam name="T">
    ///     Type of IdentityBase.
    /// </typeparam>
    [Serializable]
    public abstract class IdentityBase<T> : IIdentity, ICheckPermissions
        where T : IdentityBase<T>
    {
        #region Fields

        /// <summary>
        ///     The authentication type.
        /// </summary>
        private string _authenticationType;

        /// <summary>
        ///     The is authenticated.
        /// </summary>
        private bool _isAuthenticated;

        /// <summary>
        ///     The name.
        /// </summary>
        private string _name;

        /// <summary>
        ///     The permissions.
        /// </summary>
        private List<Permission> _permissions;

        private Guid? _userId;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the permissions.
        /// </summary>
        public List<Permission> Permissions
        {
            get { return _permissions ?? (_permissions = new List<Permission>()); }

            protected set
            {
                if (_permissions == null || _permissions == value)
                {
                    return;
                }

                _permissions = value;
            }
        }

        public Guid? UserId
        {
            get { return _userId; }

            protected set
            {
                if (value == null || _userId == value)
                {
                    return;
                }

                _userId = value;
            }
        }

        /// <summary>
        ///     Gets or sets the authentication type.
        /// </summary>
        public string AuthenticationType
        {
            get { return _authenticationType; }

            protected set
            {
                if (_authenticationType == null || _authenticationType != value)
                {
                    _authenticationType = value;
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether is authenticated.
        /// </summary>
        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }

            protected set { _isAuthenticated = value; }
        }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name
        {
            get { return _name; }

            protected set { _name = value; }
        }

        #endregion

        #region Public Methods and Operators

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
            return true;
        }

        public bool CanSeeMenu()
        {
            return true;
        }

        #endregion
    }
}