using System.CommandLine;

using ExtSort.Models;
using ExtSort.Models.Arguments;

namespace ExtSort.Services.Factories
{
    internal static class ArgumentFactory
    {
        public static Lazy<Dictionary<string, Argument>> GeneratorArguments = new (BuildGenerator().BuildArguments);
        public static Lazy<Dictionary<string, Argument>> SorterArguments = new (BuildSorter().BuildArguments);

        private static IEnumerable<Argument> BuildSorter()
        {
            yield return new ExtSortArgument<string>("src_file_name", "Source file with unsorted output")
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(SorterArgument.SourceFileName)
            };
            yield return new ExtSortArgument<string>("dst_file_name", "Destination file with sorted output")
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(SorterArgument.TargetFileName)
            };
        }

        private static IEnumerable<Argument> BuildGenerator()
        {
            yield return new ExtSortArgument<string>("src_file_name", "Name of a file")
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(GeneratorArgument.TargetFileName)
            };
            yield return new ExtSortArgument<string>("file_size_kb", "Size of a file (kb)")
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(GeneratorArgument.TargetFileSizeKb)
            };
        }

        private static Dictionary<string, Argument> BuildArguments(this IEnumerable<Argument> arguments)
        {
            return arguments.ToDictionary(
                arg =>
                {
                    var prop = arg as IPropertyInfo;
                    return prop?.TargetPropertyName
                        ?? throw new InvalidOperationException($"Target property is not defined for the {arg.Name}");
                },
                arg => arg);
        }
    }
}
