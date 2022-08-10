using System;

namespace past.ConsoleApp.Wrappers
{
    /// <inheritdoc cref="AppDomain"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="AppDomain"/> to support mocking.
    /// Only the methods used by this project are exposed through this wrapper.
    /// </remarks>
    public interface IAppDomainWrapper
    {
        /// <inheritdoc cref="AppDomain.ProcessExit"/>
        event EventHandler ProcessExit;
    }
}
