// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICheckPermissions.cs" company="B. F. Saul Co.">
//   © 2014 B. F. Saul Co.
// </copyright>
// <summary>
//   Interface defining an object that
//   checks HasPermission.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WebDAVSharp.Data.Security
{
    /// <summary>
    ///     Interface defining an object that
    ///     checks HasPermission.
    /// </summary>
    public interface ICheckPermissions
    {
        #region Public Methods and Operators

        /// <summary>
        ///     The has permission.
        /// </summary>
        /// <param name="permission">
        ///     The permission.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        bool HasPermission(PermissionType permission);

        bool CanSeeMenu();

        #endregion
    }
}