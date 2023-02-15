using past.ConsoleApp.Commands;
using past.ConsoleApp.Extensions;
using past.ConsoleApp.Middleware;
using past.Core;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace past.ConsoleApp
{
    public class Program
    {
        [ExcludeFromCodeCoverage]
        public static Task<int> Main(string[] args)
        {
#if DEBUG
            // In debug builds halt execution until a key is pressed if the debug flag was provided to allow attaching a debugger.
            // Check for presence of the debug flag even before any argument parsing is done so that code can be debugged if needed as well.
            if (args.Contains("--debug"))
            {
                var originalForeground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine("DEBUG: Ready to attach debugger. Press any key to continue execution...");
                Console.ForegroundColor = originalForeground;
                Console.ReadKey(intercept: true); // Suppress printing the pressed key
            }
#endif // DEBUG

            return MainInternal(args);
        }

        protected internal static async Task<int> MainInternal(string[] args, PastCommand? rootCommand = null, IConsoleModeMiddleware? consoleModeMiddleware = null, IConsole? console = null)
        {
            rootCommand ??= new PastCommand();
            var commandLineBuilder = new CommandLineBuilder(rootCommand);
            commandLineBuilder.UseSuggestDirective();
            commandLineBuilder.RegisterWithDotnetSuggest();
            commandLineBuilder.CancelOnProcessTermination();
            commandLineBuilder.UseParseErrorReporting((int)ErrorCode.ParseError);
            commandLineBuilder.UseExceptionHandler(errorExitCode: (int)ErrorCode.UnexpectedError);
            commandLineBuilder.UseVersionOption("--version", "-V");
            commandLineBuilder.UseHelp(CustomizeHelp);

            if (args.Contains("--ansi"))
            {
                consoleModeMiddleware ??= new ConsoleModeMiddleware();
                commandLineBuilder.AddMiddleware(consoleModeMiddleware.ConfigureConsoleMode);
            }

            var commandLineParser = commandLineBuilder.Build();
            return await commandLineParser.InvokeAsync(args, console);
        }

        /// <summary>
        /// Customizes new and existing help sections.
        /// </summary>
        /// <param name="context">The help context.</param>
        private static void CustomizeHelp(HelpContext context)
        {
            if (context.Command.Name == "past")
            {
                // Add a note to help option description on the root command about using help for subcommands
                var helpOption = context.Command.Options.First(option => option.Name == "help");
                helpOption.Description += ". Use with a subcommand to show help specific to that command.";

                // Add a note to the pin command description on the root command to see command help for warnings
                var pinCommand = context.Command.Subcommands.First(subcommand => subcommand.Name == "pin");
                pinCommand.Description += " (see command help for experimental warnings).";
            }

            // Expand description for command help to include warnings
            if (context.Command.Name == "pin")
            {
                context.Command.Description += ". \nWARNING:" +
                    "\n  - Items pinned this way will not show as pinned in the clipboard history UI until the clipboard service is restarted (which will clear non-pinned items from clipboard history)." +
                    "\n  - This does NOT encrypt the content of the item when storing it to disk, unlike pinning via the clipboard history UI." +
                    "\n  - This may be lossy for clipboard items containing multiple data formats when the item is restored from disk after a reboot (ie. text with formatting would retain only the text).";
            }

            context.HelpBuilder.CustomizeLayout(_ =>
            {
                return HelpBuilder.Default.GetLayout()
                .Prepend(_ =>
                {
                    // Include a header at the top of help for all commands
                    _.Output.Write("past ");
                    if (_.Command.Name == "past")
                    {
                        // For the root command include the version and an overall description of the tool itself
                        _.Output.WriteLine(GetVersion());
                        _.Output.WriteLine("A CLI for interacting with Windows Clipboard History.");
                    }
                    else
                    {
                        // For subcommands just include the command name
                        _.Output.WriteLine(_.Command.Name);
                    }
                })
                .Append(_ =>
                {
                    // Add an extra empty line before the next help section for the root command.
                    // For some reason the root commands seems to be the only one that doesn't already include an empty line here.
                    if (_.Command.Name == "past")
                    {
                        _.Output.WriteLine();
                    }

                    // Only show the exit codes when the full help is requested with the long help flag
                    if (_.ParseResult.Tokens.Any(token => token.Value == "--help"))
                    {
                        _.Output.WriteLine("Exit Codes:");

                        // Add the appropriate success exit codes per command
                        var lines = new List<TwoColumnHelpRow>();
                        if (_.Command.Name == "list")
                        {
                            lines.AddExitCodeHelpRow("Success", "The number of items returned");
                        }
                        else if (_.Command.Name == "status")
                        {
                            lines.AddExitCodeHelpRow("History Enabled, Roaming Enabled", "3");
                            lines.AddExitCodeHelpRow("History Disabled, Roaming Enabled", "2");
                            lines.AddExitCodeHelpRow("History Enabled, Roaming Disabled", "1");
                            lines.AddExitCodeHelpRow("History Disabled, Roaming Disabled", "0");
                        }
                        else
                        {
                            lines.AddExitCodeHelpRow(ErrorCode.Success);
                        }

                        // ParseError is common to all commands
                        lines.AddExitCodeHelpRow(ErrorCode.ParseError);

                        if (_.Command.Name == "get" || _.Command.Name == "list")
                        {
                            // NotFound is common to both `get` and `list`
                            lines.AddExitCodeHelpRow(ErrorCode.NotFound);

                            if (_.Command.Name == "get")
                            {
                                // IncompatibleContentType is specific to `get`
                                lines.AddExitCodeHelpRow(ErrorCode.IncompatibleContentType);
                            }

                            // ClipboardHistoryDisabled and AccessDenied are common to both `get` and `list`
                            lines.AddExitCodeHelpRow(ErrorCode.ClipboardHistoryDisabled);
                            lines.AddExitCodeHelpRow(ErrorCode.AccessDenied);
                        }

                        // UnexpectedError is common to all commands
                        lines.AddExitCodeHelpRow(ErrorCode.UnexpectedError);

                        _.HelpBuilder.WriteColumns(lines, _);
                    }
                    else
                    {
                        // Include a note about how to show help with exit codes when a short help flag is used
                        _.Output.WriteLine("Note: Use `--help` to show the full help including exit codes.");
                    }
                });
            });
        }

        /// <summary>
        /// Gets the informational version from the executing assemebly, or the assembly version if the informational version is not found.
        /// </summary>
        /// <returns>The informational version if available, otherwise the assembly version.</returns>
        private static string GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            return assemblyVersionAttribute!.InformationalVersion;
        }
    }
}
