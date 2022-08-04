using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace past.ConsoleApp.Commands
{
    /// <summary>
    /// Prints help for the given command, or full help if no command is given.
    /// </summary>
    public class HelpCommand : Command
    {
        /// <summary>
        /// Creates a new <see cref="HelpCommand"/> with the given handler.
        /// </summary>
        /// <param name="handler">Command handler</param>
        public HelpCommand(Func<string, Task> handler)
            : base("help")
        {
            var commandArgument = new Argument<string>("command");
            commandArgument.SetDefaultValue(string.Empty);
            commandArgument.AddCompletions("list", "get", "status");
            this.AddArgument(commandArgument);
            this.SetHandler(handler,
                commandArgument);
        }
    }
}
