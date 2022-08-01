using System.Diagnostics.CodeAnalysis;

namespace past.ConsoleApp
{
    public class ConsoleUtilities : IConsoleUtilities
    {
        public bool TryEnableVirtualTerminalProcessing([NotNullWhen(false)] out string? error)
            => ConsoleHelpers.TryEnableVirtualTerminalProcessing(out error);
    }
}
