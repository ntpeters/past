using System.CommandLine.Invocation;

namespace past.ConsoleApp.Middleware
{
    /// <summary>
    /// Supports configuring the console mode as part of the command invocation pipeline.
    /// </summary>
    public interface IConsoleModeMiddleware
    {
        /// <summary>
        /// Enables virtual terminal processing in the current console, and ensures
        /// the original console mode is restored on process exit.
        /// </summary>
        /// <remarks>
        /// If output is redirected, calling this method has no effect.
        /// </remarks>
        /// <param name="context">The context for the current invocation.</param>
        void ConfigureConsoleMode(InvocationContext context);
    }
}
