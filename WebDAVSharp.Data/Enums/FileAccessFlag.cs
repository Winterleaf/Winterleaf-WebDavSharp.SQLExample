namespace WebDAVSharp.Data.Enums
{
    public enum FileAccessFlag
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4,
        FullControl = Read & Write & Delete
    }
}