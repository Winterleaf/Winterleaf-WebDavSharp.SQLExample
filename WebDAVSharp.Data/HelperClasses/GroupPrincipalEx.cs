using System.DirectoryServices.AccountManagement;

namespace WebDAVSharp.Data.HelperClasses
{
    [DirectoryObjectClass("group")]
    [DirectoryRdnPrefix("CN")]
    public class GroupPrincipalEx : GroupPrincipal
    {
        public GroupPrincipalEx(PrincipalContext context) : base(context)
        {
        }

        public GroupPrincipalEx(PrincipalContext context, string samAccountName)
            : base(context, samAccountName)
        {
        }

        [DirectoryProperty("mail")]
        public string EmailAddress
        {
            get
            {
                if (ExtensionGet("mail").Length != 1)
                    return null;

                return (string) ExtensionGet("mail")[0];
            }
            set { ExtensionSet("mail", value); }
        }
    }
}