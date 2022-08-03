using System;

namespace past.ConsoleApp.Wrappers
{
    /// <inheritdoc cref="IEnvironmentWrapper"/>
    public class EnvironmentWrapper : IEnvironmentWrapper
    {
        public string? GetEnvironmentVariable(string variable)
            => Environment.GetEnvironmentVariable(variable);
    }
}
