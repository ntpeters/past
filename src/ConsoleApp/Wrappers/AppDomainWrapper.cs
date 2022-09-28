using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace past.ConsoleApp.Wrappers
{
    /// <inheritdoc cref="IAppDomainWrapper"/>
    [ExcludeFromCodeCoverage(Justification = "Wrappers are not intended to be tested, as they exist solely to enable dependency injection of non-mockable APIs.")]
    public class AppDomainWrapper : IAppDomainWrapper
    {
        public event EventHandler ProcessExit
        {
            add
            {
                // Lazily subscribe to the underlaying event once someone subscribes to this event.
                // This way we don't have to worry about doing anything special to cleanup our own event subscriptions here to prevent leaks.
                if (ProcessExitInternal == null || ProcessExitInternal?.GetInvocationList().Length == 0)
                {
                    AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
                }
                ProcessExitInternal += value;
            }

            remove
            {
                // Lazily unsubscribe from the underlaying event once the last subscriber unsubscribes from this event.
                // This way we don't have to worry about doing anything special to cleanup our own event subscriptions here to prevent leaks.
                ProcessExitInternal -= value;
                if (ProcessExitInternal == null || ProcessExitInternal?.GetInvocationList().Length == 0)
                {
                    AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
                }
            }
        }

        // Internal event used to allow lazy subscribing to the underlaying event only when needed
        private event EventHandler? ProcessExitInternal;

        /// <summary>
        /// Event handler delegate that bridges events between <see cref="AppDomain.ProcessExit"/>
        /// and <see cref="ProcessExit"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">An object that contains no event data.</param>
        private void OnProcessExit(object? sender, EventArgs args)
        {
            // Ensure we're always raising to the most recent set of subscribers
            Interlocked.CompareExchange(ref ProcessExitInternal, null, null)?.Invoke(sender, args);
        }
    }
}
