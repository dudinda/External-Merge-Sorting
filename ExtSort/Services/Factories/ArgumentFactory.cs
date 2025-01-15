using ExtSort.Code.Enums;
using ExtSort.Models;
using ExtSort.Models.Binders;

using System.CommandLine;

namespace ExtSort.Services.Factories
{
    internal static class ArgumentFactory
    {
        public static Lazy<Dictionary<string, Argument>> GeneratorArguments = new (BuildGenerator().BuildArguments);
        public static Lazy<Dictionary<string, Argument>> SorterArguments = new (BuildSorter().BuildArguments);
        public static Lazy<Dictionary<string, Argument>> EvaluatorArguments = new(BuildEvaluator().BuildArguments);

        private static IEnumerable<Argument> BuildSorter()
        {
            yield return new ExtSortArgument<string>("src_file_name", "Source file with unsorted output")
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(SorterBinder.SourceFileName)
            };
            yield return new ExtSortArgument<string>("dst_file_name", "Destination file with sorted output")
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(SorterBinder.TargetFileName)
            };
            yield return new ExtSortArgument<SortMode>("sort_mode", () => SortMode.CPU, "A mode to sort an output")
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(SorterBinder.Mode)
            };
        }

        private static IEnumerable<Argument> BuildGenerator()
        {
            yield return new ExtSortArgument<string>("src_file_name", "Name of a file")
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(GeneratorBinder.TargetFileName)
            };
            yield return new ExtSortArgument<string>("file_size_kb", "Size of a file (kb)")
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(GeneratorBinder.TargetFileSizeKb)
            };
        }

        private static IEnumerable<Argument> BuildEvaluator()
        {
            yield return new ExtSortArgument<string>("file_size_mb", "Size of a file (mb)") 
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(EvaluatorBinder.FileSizeMb)
            };
            yield return new ExtSortArgument<string>("ram_available_mb", "RAM available (mb)") 
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(EvaluatorBinder.RamAvailableMb)
            };
            yield return new ExtSortArgument<string>("number_of_files", "The number of sorted files to be merged") 
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(EvaluatorBinder.NumberOfFiles)
            };
            yield return new ExtSortArgument<string>("disk_random_read_speed", "Random access reading speed of a disk (mb/s)")
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(EvaluatorBinder.DiskRandomReadSpeedMbs)
            };
            yield return new ExtSortArgument<string>("disk_latency", "Latency of a disk (ms)") 
            {
                Arity = ArgumentArity.ExactlyOne,
                TargetPropertyName = nameof(EvaluatorBinder.DiskLatencyMs)
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
