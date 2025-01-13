using ExtSort.Services.Factories;
using System.CommandLine.Parsing;

namespace ExtSort.Models.Binders 
{
    internal record GeneratorBinder
    {
        public GeneratorBinder(ParseResult parser) 
        {
            var args = ArgumentFactory.GeneratorArguments.Value;
         
            var size = parser.GetValueForArgument(args[nameof(TargetFileSizeKb)])?.ToString();
            if (!long.TryParse(size?.ToString(), out var fileSize))
                throw new InvalidCastException("The length of the output file is in incorrect format.");
            TargetFileSizeKb = fileSize;
            TargetFileName = (string)parser.GetValueForArgument(args[nameof(TargetFileName)]);
        }

        public string TargetFileName { get; init; }
        public long TargetFileSizeKb { get; init; }
    }
}
