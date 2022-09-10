using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;

namespace past.ConsoleApp.Test.TestHelpers
{
    /// <summary>
    /// Extensions for <see cref="Command"/> to support testing.
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        /// Gets a command's handler that was set with <see cref="Handler.SetHandler>"/>.
        /// </summary>
        /// <remarks>
        /// This is a counterpart to <see cref="Handler.SetHandler>"/> to allow directly testing that the handler
        /// was correctly set on derived commands without needing to invoke the command.
        /// <br/>
        /// The handle is looked up using reflection since it is held in a private field on an internal type.
        /// </remarks>
        /// <param name="command"><see cref="Command"/> to get the handler from.</param>
        /// <returns>The original handle provided to <see cref="Handler.SetHandler>"/> for the command.</returns>
        public static object? GetHandler(this Command command)
        {
            // Load the assembly and get the type for the internal command handler used by `Command.SetHandler`
            var systemCommandLineAssembly = Assembly.Load("System.CommandLine");
            var anonymousCommandHandlerType = systemCommandLineAssembly.GetType("System.CommandLine.Invocation.AnonymousCommandHandler");

            // Get the value of the private field on the internal handler holding the wrapped handle
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var handleFieldInfo = anonymousCommandHandlerType?.GetField("_handle", bindingFlags);
            var anonymousHandlerHandle = (Func<InvocationContext, Task>?)handleFieldInfo?.GetValue(command.Handler);

            // Finally, get and return the value of the original handle provided to `Command.SetHandler`
            return GetHandlerDelegateTarget(anonymousHandlerHandle);
        }

        /// <summary>
        /// Recursively searches the given handler delegate for the last handle in the wrapped handle hierarchy.
        /// </summary>
        /// <remarks>
        /// This is needed since it appears that the command handler is wrapped a different number of times depending
        /// on whether the original handle is an <see cref="Action"/> or <see cref="Func{T, TResult}"/>.
        /// </remarks>
        /// <param name="handlerDelegate">Delegate to search for the last handle of.</param>
        /// <returns>The final handle in the heiarchy of wrapped handles for the given delegate.</returns>
        private static object? GetHandlerDelegateTarget(object? handlerDelegate)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            // Get the `Target` property of the delegate
            var handlerDelegateType = handlerDelegate?.GetType();
            var handlerDelegateTargetPropertyInfo = handlerDelegateType?.GetProperty("Target", bindingFlags);
            var handlerDelegateTarget = handlerDelegateTargetPropertyInfo?.GetValue(handlerDelegate);

            // Try to get the field holding the next handle in the hierarchy
            var handlerDelgateTargetType = handlerDelegateTarget?.GetType();
            var handlerDelegateTargetHandleFieldInfo = handlerDelgateTargetType?.GetField("handle", bindingFlags);
            var handlerDelegateTargetHandle = handlerDelegateTargetHandleFieldInfo?.GetValue(handlerDelegateTarget);

            // If the delegate target does not contain a `handle` field, then we've found the final handle
            if (handlerDelegateTargetHandle == null)
            {
                return handlerDelegate;
            }

            // Continue recursing through the handler delegates until we find the final handle
            return GetHandlerDelegateTarget(handlerDelegateTargetHandle);
        }
    }
}
