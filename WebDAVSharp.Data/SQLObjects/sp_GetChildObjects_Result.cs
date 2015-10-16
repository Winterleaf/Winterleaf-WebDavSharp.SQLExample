using System;

namespace WebDAVSharp.Data.SQLObjects
{
    public class sp_GetChildObjects_Result
    {
        public Guid objectguid { get; set; }
        public bool isCollection { get; set; }
    }
}