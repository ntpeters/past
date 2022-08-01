using System.Diagnostics.CodeAnalysis;

namespace past.ConsoleApp
{
    public interface IConsoleUtilities
    {
        bool TryEnableVirtualTerminalProcessing([NotNullWhen(false)] out string? error);
    }
}
