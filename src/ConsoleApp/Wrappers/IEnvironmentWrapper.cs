using System;

namespace past.ConsoleApp.Wrappers
{
    /// <inheritdoc cref="Environment"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="Environment"/> to support mocking.
    /// Only the methods used by this project are exposed through this wrapper.
    /// </remarks>
    public interface IEnvironmentWrapper
    {
        /// <inheritdoc cref="Environment.GetEnvironmentVariable(string)"/>
        string? GetEnvironmentVariable(string variable);
    }
}
