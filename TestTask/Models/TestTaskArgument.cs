using System.CommandLine;

namespace TestTask.Models
{
    internal class TestTaskArgument<T> : Argument<T>, IPropertyInfo
    {
        public string TargetPropertyName { get; init; }

        public TestTaskArgument(
           string name,
           string description = null) : base(name, description)
        {

        }

        public TestTaskArgument(
            string name,
            Func<T> getDefaultValue,
            string description = null) : base(name, getDefaultValue, description)
        {

        }
    }
}
