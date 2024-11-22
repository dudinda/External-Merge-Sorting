using System.CommandLine;

namespace ExtSort.Models
{
    internal class ExtSortArgument<T> : Argument<T>, IPropertyInfo
    {
        public string TargetPropertyName { get; init; }

        public ExtSortArgument(
           string name,
           string description = null) : base(name, description)
        {

        }

        public ExtSortArgument(
            string name,
            Func<T> getDefaultValue,
            string description = null) : base(name, getDefaultValue, description)
        {

        }
    }
}
