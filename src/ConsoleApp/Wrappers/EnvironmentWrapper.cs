using System;
using System.Diagnostics.CodeAnalysis;

namespace past.ConsoleApp.Wrappers
{
    /// <inheritdoc cref="IEnvironmentWrapper"/>
    [ExcludeFromCodeCoverage(Justification = "Wrappers are not intended to be tested, as they exist solely to enable dependency injection of non-mockable APIs.")]
    public class EnvironmentWrapper : IEnvironmentWrapper
    {
        public string? GetEnvironmentVariable(string variable)
            => Environment.GetEnvironmentVariable(variable);
    }
}
