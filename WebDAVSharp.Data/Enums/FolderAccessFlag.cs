namespace WebDAVSharp.Data.Enums
{
    public enum FolderAccessFlag
    {
        None = 0,
        ListFolder = 1,
        CreateFiles = 2,
        CreateFolders = 4,
        Delete = 8,
        ChangePermissions = 16,
        FullControl = ListFolder & CreateFiles & CreateFolders & Delete & ChangePermissions
    }
}