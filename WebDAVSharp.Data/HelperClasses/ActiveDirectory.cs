using System;
using System.Data.Entity;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;

namespace WebDAVSharp.Data.HelperClasses
{
    public static class ActiveDirectory
    {
        static ActiveDirectory()
        {
            Domain domain = null;
            DomainController domainController = null;
            try
            {
                domain = Domain.GetCurrentDomain();
                DomainName = domain.Name;
                domainController = domain.PdcRoleOwner;
                DomainControllerName = domainController.Name.Split('.')[0];
                ComputerName = Environment.MachineName;
            }
            finally
            {
                domain?.Dispose();
                domainController?.Dispose();
            }
        }

        public static string DomainControllerName { get; private set; }

        public static string ComputerName { get; private set; }

        public static string DomainName { get; }

        public static string DomainPath
        {
            get
            {
                bool bFirst = true;
                StringBuilder sbReturn = new StringBuilder(200);
                string[] strlstDc = DomainName.Split('.');
                foreach (string strDc in strlstDc)
                {
                    if (bFirst)
                    {
                        sbReturn.Append("DC=");
                        bFirst = false;
                    }
                    else
                        sbReturn.Append(",DC=");

                    sbReturn.Append(strDc);
                }
                return sbReturn.ToString();
            }
        }

        public static string RootPath => $"LDAP://{DomainName}/{DomainPath}";

        /// <summary>
        ///     Refreshes the Security Object Table with information from Active Directory.
        /// </summary>
        public static void Syncrhonize()
        {
            DownloadAdsGroups();
            DownloadAdsUsers();
            DownloadAdsGroupsMembership();
        }

        private static void DownloadAdsGroups()
        {
            using (var context = new OnlineFilesEntities())
            using (PrincipalContext ctx = new PrincipalContext(ContextType.Domain))
            using (var groupPrincipal = new GroupPrincipalEx(ctx))
            using (PrincipalSearcher search = new PrincipalSearcher(groupPrincipal))
            {
                int max = search.FindAll().Count();
                int c = 0;
                foreach (var gp in search.FindAll().Select(found => found as GroupPrincipalEx))
                {
                    Console.WriteLine("Processing " + c + " of " + max);
                    c++;
                    if (gp != null)
                    {
                        if (gp.IsSecurityGroup != true && gp.GroupScope == GroupScope.Local)
                            continue;
                        var so = context.SecurityObjects.FirstOrDefault(d => d.ActiveDirectoryId == gp.Guid);
                        if (so == null)
                        {
                            so = new SecurityObject
                            {
                                ActiveDirectoryId = gp.Guid,
                                FullName = gp.Name,
                                Username = gp.SamAccountName,
                                EmailAddress = gp.EmailAddress ?? "",
                                IsGroup = true,
                                LastLogInOn = DateTime.Now,
                                IsActive = true,
                                HomeFolder = null
                            };
                            context.SecurityObjects.Add(so);
                        }
                        else
                        {
                            so.IsGroup = true;
                            so.FullName = gp.Name;
                            so.Username = gp.SamAccountName;
                            so.EmailAddress = gp.EmailAddress ?? "";
                        }
                    }
                    context.SaveChanges();
                }
            }
        }

        private static void DownloadAdsGroupsMembership()
        {
            using (var context = new OnlineFilesEntities())
            using (PrincipalContext ctx = new PrincipalContext(ContextType.Domain))
            using (var groupPrincipal = new GroupPrincipalEx(ctx))
            using (PrincipalSearcher search = new PrincipalSearcher(groupPrincipal))
            {
                int max = search.FindAll().Count();
                int c = 0;
                foreach (var gp in search.FindAll().Select(found => found as GroupPrincipalEx))
                {
                    Console.WriteLine("Processing " + c + " of " + max);
                    c++;
                    if (gp != null)
                    {
                        if (gp.IsSecurityGroup != true && gp.GroupScope == GroupScope.Local)
                            continue;
                        var so = context.SecurityObjects.Include(d => d.MyGroups).FirstOrDefault(d => d.ActiveDirectoryId == gp.Guid);
                        if (so == null)
                            throw new Exception("Not Possible");

                        context.SecurityObjectMemberships.RemoveRange(context.SecurityObjectMemberships.Where(d => d.SecurityObjectId == so.SecurityObjectId));
                        context.SaveChanges();

                        try
                        {
                            foreach (Principal grp in gp.GetGroups())
                            {
                                var og = context.SecurityObjects.FirstOrDefault(d => d.ActiveDirectoryId == grp.Guid);
                                if (og != null)
                                    context.SecurityObjectMemberships.Add(new SecurityObjectMembership {OwnerSecurityObject = so, GroupSecurityObjectId = og.SecurityObjectId});
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    context.SaveChanges();
                }
            }
        }

        private static void DownloadAdsUsers()
        {
            using (var context = new OnlineFilesEntities())
            using (PrincipalContext ctx = new PrincipalContext(ContextType.Domain))
            using (var filter = new UserPrincipal(ctx))
            {
                using (PrincipalSearcher search = new PrincipalSearcher(filter))
                {
                    int max = search.FindAll().Count();
                    int c = 0;
                    foreach (var principal in search.FindAll())
                    {
                        Console.WriteLine("Processing " + c + " of " + max);
                        c++;
                        var user = principal as UserPrincipal;
                        if (user == null)
                            continue;
                        if (user.StructuralObjectClass == "group" && user.Enabled != true)
                            continue;


                        var so = context.SecurityObjects
                            .FirstOrDefault(d => d.ActiveDirectoryId == user.Guid);
                        if (so == null)
                        {
                            so = new SecurityObject
                            {
                                ActiveDirectoryId = user.Guid,
                                FullName = user.Name,
                                Username = user.SamAccountName,
                                EmailAddress = user.EmailAddress ?? "",
                                IsGroup = false,
                                LastLogInOn = DateTime.Now,
                                IsActive = true,
                                HomeFolder = null
                            };
                            context.SecurityObjects.Add(so);
                        }
                        else
                        {
                            so.IsGroup = false;
                            so.FullName = user.Name;
                            so.Username = user.SamAccountName;
                            so.EmailAddress = user.EmailAddress ?? "";
                        }

                        context.SaveChanges();

                        context.SecurityObjectMemberships.RemoveRange(context.SecurityObjectMemberships.Where(d => d.SecurityObjectId == so.SecurityObjectId));
                        context.SaveChanges();

                        try
                        {
                            foreach (Principal grp in user.GetGroups())
                            {
                                var og = context.SecurityObjects.FirstOrDefault(d => d.ActiveDirectoryId == grp.Guid);
                                if (og != null)
                                    context.SecurityObjectMemberships.Add(new SecurityObjectMembership {OwnerSecurityObject = so, GroupSecurityObjectId = og.SecurityObjectId});
                            }
                        }
                        catch (Exception)
                        {
                        }


                        context.SaveChanges();
                    }
                }
            }
        }
    }
}