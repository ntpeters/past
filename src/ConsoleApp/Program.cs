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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace past.ConsoleApp
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
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

            var rootCommand = new PastCommand();
            var commandLineBuilder = new CommandLineBuilder(rootCommand);
            commandLineBuilder.UseSuggestDirective();
            commandLineBuilder.RegisterWithDotnetSuggest();
            commandLineBuilder.UseParseErrorReporting((int)ErrorCode.ParseError);

            commandLineBuilder.UseHelp(CustomizeHelp);

            commandLineBuilder.UseVersionOption("--version", "-V");
            commandLineBuilder.CancelOnProcessTermination();
            commandLineBuilder.UseExceptionHandler(errorExitCode: (int)ErrorCode.UnexpectedError);

            if (args.Contains("--ansi"))
            {
                var consoleModeMiddleware = new ConsoleModeMiddleware();
                commandLineBuilder.AddMiddleware(consoleModeMiddleware.ConfigureConsoleMode);
            }

            var commandLineParser = commandLineBuilder.Build();
            return await commandLineParser.InvokeAsync(args);
        }

        /// <summary>
        /// Customizes new and existing help sections.
        /// </summary>
        /// <param name="context">The help context.</param>
        private static void CustomizeHelp(HelpContext context)
        {
            // Add a note to help option description on the root command about using help for subcommands
            if (context.Command.Name == "past")
            {
                var helpOption = context.Command.Options.First(option => option.Name == "help");
                helpOption.Description += ". Use with a subcommand to show help specific to that command.";
            }

            context.HelpBuilder.CustomizeLayout(_ =>
            {
                return HelpBuilder.Default.GetLayout().Prepend(_ =>
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

            if (assemblyVersionAttribute is null)
            {
                return assembly.GetName().Version?.ToString() ?? "";
            }
            else
            {
                return assemblyVersionAttribute.InformationalVersion;
            }
        }
    }
}
