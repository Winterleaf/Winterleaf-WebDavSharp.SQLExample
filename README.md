# Winterleaf-WebDavSharp.SQLExample

#A SQL Server base Web DAV server with permissions, etc

!!!!!!!!IMPORTANT!!!!!!!
This application only works in 64 Bit Mode.
!!!!!!!!IMPORTANT!!!!!!!

WE DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE, INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS[,][.] 
IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY SPECIAL, INDIRECT OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM 
LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH
 THE USE OR PERFORMANCE OF THIS SOFTWARE.]

To Set up this project you need to deploy the following SQL Projects to a MS SQL Server 2008 or greater server:

-WebDAVSharp.SQL.Database.Catalog

-WebDAVSharp.SQL.Database.File

Once they are deployed you need to do some initial setup.

# Step 1 - App.Config

Open the "App.Config"  in the 'WebDAVSharp.SQL' Project and add the connection string to the "WebDAVSharp.SQL.Database.File" database

  <connectionStrings>
    <!--**************************************************************************************************************************-->
    <!--**************************************************************************************************************************-->
    <!--**************************************************************************************************************************-->
    <!--Please correct this connectionstring to where you published the WebDAVSharp.SQL.Database.File database.-->
    <!--<add 
    name="OnlineFilesEntities" 
    connectionString="metadata=res://*/OnlineFiles.csdl|res://*/OnlineFiles.ssdl|res://*/OnlineFiles.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=!!!!!!SERVER NAME!!!!!;initial catalog=!!!!DATABASE NAME!!!!!;persist security info=True;user id=!!!!USER ID!!!!!;password=!!!!PASSWORD!!!!;MultipleActiveResultSets=True;App=EntityFramework&quot;" 
    providerName="System.Data.EntityClient"/>-->
    <!--**************************************************************************************************************************-->
    <!--**************************************************************************************************************************-->
    <!--**************************************************************************************************************************-->
  </connectionStrings>

# Step 2 - Default Catalog Collection

  Create a default Catalog Collection for storing the file data

  Run the following Query on the "WebDAVSharp.SQL.Database.File" database

  insert into [CatalogCollection] ([Name]) values ('Default Catalog Collection');

# Step 3 - Catalog

  Create the actual Catalog.

  insert into [Catalog] ([fk_CatalogCollectionId], [Name], [Offline], [Server], [DatabaseName], [UserName], [Password], [fk_CatalogStatusId])
select 
		pk_CatalogCollectionID					[fk_CatalogCollectionId],
		'Default Catalog'						[Name],
		0										[Offline],
		'Name of SQL Server'					[Server],
		'Name of the Database for the Catalog'	[DatabaseName],
		'Username to connect as'				[UserName],
		'Password to use'						[Password],
		1										[fk_CatalogStatusId]
from
	CatalogCollection;

# Step 4 - Root Folder

Create the root folder that everything is based off.

insert into [Folder]
(pk_FolderId, fk_ParentFolderId, Name, CreateDt, fk_CatalogCollectionId, Win32FileAttribute, isDeleted, DeletedDt, DeletedBy, Owner, CreatedBy)
select top 1
	'00000000-0000-0000-0000-000000000000',
	null,
	'Root',
	getdate(),
	pk_CatalogCollectionID,
	16,
	0,
	null,
	null,
	'00000000-0000-0000-0000-000000000000',
	'00000000-0000-0000-0000-000000000000'
from 
	[CatalogCollection];

# Step 5 - Active Directory

Syncronize the Active Directory Domain accounts to the database
This process works identical to how Microsoft Sharepoint does.... But simpler

From the command prompt where the executable is:

WebDAVSharp.SQL.exe -SyncADS

# Step 6 - If you want to be able to access it from another computer you need to change the ServiceImplementation.cs

 public class ServiceImplementation : IWindowsService
    {
        private const string Url = "http://localhost:8880/";

        
Change the URL to the name of your computer.

# Step 7 - Run the application in Debug mode.

WebDAVSharp.SQL.exe -Debug


# Step 8 - Test

From the run command type \\localhost:8880\WebDavRoot\

If all is working you will see a network share.
