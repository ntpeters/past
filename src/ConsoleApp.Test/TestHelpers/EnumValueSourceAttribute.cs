using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System.Collections;

namespace past.ConsoleApp.Test.TestHelpers
{
    /// <summary>
    /// Indicates that a parameter of an enumeration type on a test method will have the values of the enumeration provided for its data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class EnumValueSourceAttribute : NUnitAttribute, IParameterDataSource
    {
        public IEnumerable GetData(IParameterInfo parameter)
        {
            if (!parameter.ParameterType.IsEnum)
            {
                throw new InvalidDataSourceException("The type of the parameter EnumValueSourceAttribute is applied to must be an enumeration type.");
            }

            foreach (var enumValue in Enum.GetValues(parameter.ParameterType))
            {
                yield return enumValue;
            }
        }
    }
}
