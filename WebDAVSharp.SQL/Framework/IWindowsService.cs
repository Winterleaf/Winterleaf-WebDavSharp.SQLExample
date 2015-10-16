using System;

namespace WebDAVSharp.SQL.Framework
{
    /// <summary>
    ///     The interface that any windows service should implement to be used
    ///     with the GenericWindowsService executable.
    /// </summary>
    public interface IWindowsService : IDisposable
    {
        #region Public Methods and Operators

        /// <summary>
        ///     This method is called when a service gets a request to resume
        ///     after a pause is issued.
        /// </summary>
        void OnContinue();

        /// <summary>
        ///     This method is called when a service gets a request to pause,
        ///     but not stop completely.
        /// </summary>
        void OnPause();

        /// <summary>
        ///     This method is called when the machine the service is running on
        ///     is being shutdown.
        /// </summary>
        void OnShutdown();

        /// <summary>
        ///     This method is called when the service gets a request to start.
        /// </summary>
        /// <param name="args">
        ///     Any command line arguments
        /// </param>
        void OnStart(string[] args);

        /// <summary>
        ///     This method is called when the service gets a request to stop.
        /// </summary>
        void OnStop();

        #endregion
    }
}