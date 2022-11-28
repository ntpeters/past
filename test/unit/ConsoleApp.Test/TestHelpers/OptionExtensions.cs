using System.CommandLine;
using System.Reflection;

namespace past.ConsoleApp.Test.TestHelpers
{
    /// <summary>
    /// Extensions for <see cref="Option"/> to support testing.
    /// </summary>
    public static class OptionExtensions
    {
        /// <summary>
        /// Gets whether the option is a global option.
        /// </summary>
        /// <param name="option"><see cref="Option"/> to check whether it is global.</param>
        /// <returns>True if the option is global, false otherwise.</returns>
        public static bool GetIsGlobal(this Option option)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var optionType = option?.GetType();
            var optionIsGlobalPropertyInfo = optionType?.GetProperty("IsGlobal", bindingFlags);
            var optionIsGlobal = optionIsGlobalPropertyInfo?.GetValue(option) as bool?;
            return optionIsGlobal ?? false;
        }
    }
}
