using System;
using System.ServiceProcess;
using WebDAVSharp.Server;
using WebDAVSharp.Server.Stores;
using WebDAVSharp.Server.Stores.Locks.Interfaces;
using WebDAVSharp.SQL.Framework;
using WebDAVSharp.SQL.SQLStore;

#if DEBUG
using Common.Logging;
using Common.Logging.Configuration;
using Common.Logging.Simple;

#endif

namespace WebDAVSharp.SQL
{
    /// <summary>
    ///     The actual implementation of the windows service goes here...
    /// </summary>
    [WindowsService("WebDavSharp.SQL",
        DisplayName = "WebDavSharp.SQL",
        Description = "WebDavSharp.SQL",
        EventLogSource = "WebDavSharp.SQL",
        StartMode = ServiceStartMode.Automatic)]
    public class ServiceImplementation : IWindowsService
    {
        private const string Url = "http://localhost:8880/";

        public void Dispose()
        {
        }

        /// <summary>
        ///     This method is called when the service gets a request to start.
        /// </summary>
        /// <param name="args">Any command line arguments</param>
        public void OnStart(string[] args)
        {
#if DEBUG
            NameValueCollection properties = new NameValueCollection {["showDateTime"] = "true"};
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(properties);
#endif
            IWebDavStoreItemLock lockSystem = new WebDavSqlStoreItemLock();
            IWebDavStore store = new WebDavSqlStore("\\Data", new Guid("00000000-0000-0000-0000-000000000000"), lockSystem);
            WebDavServer server = new WebDavServer(ref store, AuthType.Negotiate);

            server.Start(Url);
        }

        /// <summary>
        ///     This method is called when the service gets a request to stop.
        /// </summary>
        public void OnStop()
        {
        }

        /// <summary>
        ///     This method is called when a service gets a request to pause,
        ///     but not stop completely.
        /// </summary>
        public void OnPause()
        {
        }

        /// <summary>
        ///     This method is called when a service gets a request to resume
        ///     after a pause is issued.
        /// </summary>
        public void OnContinue()
        {
        }

        /// <summary>
        ///     This method is called when the machine the service is running on
        ///     is being shutdown.
        /// </summary>
        public void OnShutdown()
        {
        }
    }
}