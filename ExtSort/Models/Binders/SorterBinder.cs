using ExtSort.Code.Enums;
using ExtSort.Services.Factories;

using System.CommandLine.Parsing;

namespace ExtSort.Models.Binders 
{
    internal record SorterBinder
    {
        public SorterBinder(ParseResult parser) 
        {
            var args = ArgumentFactory.SorterArguments.Value;
            TargetFileName = (string)parser.GetValueForArgument(args[nameof(TargetFileName)]);
            SourceFileName = (string)parser.GetValueForArgument(args[nameof(SourceFileName)]);
            Mode = (SortMode)parser.GetValueForArgument(args[nameof(Mode)]);
        }

        public string TargetFileName { get; init; }
        public string SourceFileName { get; init; }
        public SortMode Mode { get; init; }
    }
}
