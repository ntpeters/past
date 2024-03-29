using past.ConsoleApp.Extensions;
using past.ConsoleApp.Wrappers;
using System;
using System.CommandLine.Invocation;

namespace past.ConsoleApp.Middleware
{
    /// <inheritdoc cref="IConsoleModeMiddleware"/>
    public class ConsoleModeMiddleware : IConsoleModeMiddleware
    {
        #region Private Fields
        private readonly IConsoleUtilities _consoleUtilities;
        private readonly IAppDomainWrapper _appDomain;
        #endregion Private Fields

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="ConsoleModeMiddleware"/>.
        /// </summary>
        public ConsoleModeMiddleware()
            : this(new ConsoleUtilities(), new AppDomainWrapper())
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConsoleModeMiddleware"/> with the provided console utilities and app domain.
        /// </summary>
        /// <param name="consoleUtilities">Console utilities to use for interacting with the console.</param>
        /// <param name="appDomain">App domain to subscribe to the process exit event of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="consoleUtilities"/> or <paramref name="appDomain"/> is null.</exception>
        public ConsoleModeMiddleware(IConsoleUtilities consoleUtilities, IAppDomainWrapper appDomain)
        {
            _consoleUtilities = consoleUtilities ?? throw new ArgumentNullException(nameof(consoleUtilities));
            _appDomain = appDomain ?? throw new ArgumentNullException(nameof(appDomain));
        }
        #endregion Constructors

        #region Public Methods
        public void ConfigureConsoleMode(InvocationContext context)
        {
            if (context.Console.IsOutputRedirected)
            {
                return;
            }

            if (_consoleUtilities.TryEnableVirtualTerminalProcessing(out var error))
            {
                // Restore console mode on exit
                _appDomain.ProcessExit += (sender, args) =>
                {
                    if (!_consoleUtilities.TryRestoreConsoleMode(out var error))
                    {
                        context.Console.WriteErrorLine($"Failed to restore original console mode: {error}");
                    }
                };
            }
            else
            {
                context.Console.WriteErrorLine($"Failed to enable VT processing: {error}");
            }
        }
        #endregion Public Methods
    }
}
