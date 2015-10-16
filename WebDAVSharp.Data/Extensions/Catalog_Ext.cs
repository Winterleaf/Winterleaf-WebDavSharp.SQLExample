namespace WebDAVSharp.Data
{
    public partial class Catalog
    {
        public string EntityConnectionString => "metadata=res://*/OnlineFiles_Catalog.csdl|res://*/OnlineFiles_Catalog.ssdl|res://*/OnlineFiles_Catalog.msl;" +
                                                "provider=System.Data.SqlClient;" +
                                                "provider connection string=\";" +
                                                "data source=" + Server + ";" +
                                                "initial catalog=" + DatabaseName + ";" +
                                                "persist security info=True;" +
                                                "user id=" + UserName + ";" +
                                                "password=" + Password + ";" +
                                                "MultipleActiveResultSets=True;" +
                                                "App=EntityFramework\";";
    }
}