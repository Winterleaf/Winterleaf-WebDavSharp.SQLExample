using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using WebDAVSharp.Data;
using WebDAVSharp.Data.HelperClasses;
using WebDAVSharp.SQL.Framework;

namespace WebDAVSharp.SQL
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["LDAP_SETTING"].Value = ActiveDirectory.RootPath;
                ConsoleHarness.WriteToConsole(ConsoleColor.Yellow, "Application is linked to the Domain: " + config.AppSettings.Settings["LDAP_SETTING"].Value);

                if (config.ConnectionStrings.ConnectionStrings["OnlineFilesEntities"] == null || config.ConnectionStrings.ConnectionStrings["OnlineFilesEntities"].ConnectionString == "")
                {
                    ConsoleHarness.WriteToConsole(ConsoleColor.Red, "Connection String (OnlineFilesEntities) is not set in the WebConfig.");
                    ConsoleHarness.WriteToConsole(ConsoleColor.Red, "Hit Any Key to Exit");
                    Console.ReadKey();
                    return;
                }

                config.Save(ConfigurationSaveMode.Full);

                using (var context = new OnlineFilesEntities())
                {
                    bool hadError = false;
                    if (!context.CatalogCollections.Any())
                    {
                        ConsoleHarness.WriteToConsole(ConsoleColor.Red, "No Online Catalog Collections Created.");
                        ConsoleHarness.WriteToConsole(ConsoleColor.Yellow, "Please create an entry in the 'CatalogCollection' and 'Catalog' tables.");
                        ConsoleHarness.WriteToConsole(ConsoleColor.Yellow, @"insert into [CatalogCollection] ([Name]) values ('Default Catalog Collection');");
                        hadError = true;
                    }
                    if (!context.Catalogs.Any())
                    {
                        ConsoleHarness.WriteToConsole(ConsoleColor.Red, @"Missing a default catalog in the database.");
                        ConsoleHarness.WriteToConsole(ConsoleColor.Yellow, @"
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
	CatalogCollection;");
                        hadError = true;
                    }
                    if (hadError)
                    {
                        ConsoleHarness.WriteToConsole(ConsoleColor.Red, "Hit Any Key to Exit");
                        Console.ReadKey();
                        return;
                    }

                    if (!context.Folders.Any(d => d.pk_FolderId == new Guid()))
                    {
                        ConsoleHarness.WriteToConsole(ConsoleColor.Red, @"The Root Folder does not exist!");

                        ConsoleHarness.WriteToConsole(ConsoleColor.Yellow, @"
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
	[CatalogCollection];");

                        ConsoleHarness.WriteToConsole(ConsoleColor.Red, "Hit Any Key to Exit");
                        Console.ReadKey();
                        return;
                    }
                    if (!context.SecurityObjects.Any() && !args.Contains("-SyncADS", StringComparer.CurrentCultureIgnoreCase))
                    {
                        ConsoleHarness.WriteToConsole(ConsoleColor.Red, @"Active Directory has not been Syncronized.");
                        ConsoleHarness.WriteToConsole(ConsoleColor.Yellow, @"Run the program with option '-SyncADS'.");
                        ConsoleHarness.WriteToConsole(ConsoleColor.Red, "Hit Any Key to Exit");
                        Console.ReadKey();
                        return;
                    }
                }

                // if install was a command line flag, then run the installer at runtime.
                if (args.Contains(" - install", StringComparer.InvariantCultureIgnoreCase))
                {
                    WindowsServiceInstaller.RuntimeInstall<ServiceImplementation>();
                }

                // if uninstall was a command line flag, run uninstaller at runtime.
                else if (args.Contains("-uninstall", StringComparer.InvariantCultureIgnoreCase))
                {
                    WindowsServiceInstaller.RuntimeUnInstall<ServiceImplementation>();
                }

                // otherwise, fire up the service as either console or windows service based on UserInteractive property.
                else
                {
                    if (args.Contains("-SyncADS", StringComparer.CurrentCultureIgnoreCase))
                    {
                        ActiveDirectory.Syncrhonize();
                    }

                    var implementation = new ServiceImplementation();

                    // if started from console, file explorer, etc, run as console app.
                    if (Environment.UserInteractive)
                    {
                        ConsoleHarness.Run(args, implementation);
                    }

                    // otherwise run as a windows service
                    else
                    {
                        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                        ServiceBase.Run(new WindowsServiceHarness(implementation));
                    }
                }
            }

            catch (Exception ex)
            {
                ConsoleHarness.WriteToConsole(ConsoleColor.Red, "An exception occurred in Main(): {0}", ex);
                ConsoleHarness.WriteToConsole(ConsoleColor.Red, "Hit Any Key to Exit");
                Console.ReadKey();
            }
        }
    }
}