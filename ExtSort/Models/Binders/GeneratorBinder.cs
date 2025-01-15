using ExtSort.Services.Factories;
using System.CommandLine.Parsing;

namespace ExtSort.Models.Binders 
{
    internal record GeneratorBinder
    {
        public GeneratorBinder(ParseResult parser) 
        {
            var args = ArgumentFactory.GeneratorArguments.Value;
         
            if (!long.TryParse(parser.GetValueForArgument(args[nameof(TargetFileSizeKb)])?.ToString(), out var fileSizeKb))
                throw new InvalidCastException("The length of the output file is in incorrect format.");

            TargetFileSizeKb = fileSizeKb;
            TargetFileName = (string)parser.GetValueForArgument(args[nameof(TargetFileName)]);
        }

        public string TargetFileName { get; init; }
        public long TargetFileSizeKb { get; init; }
    }
}
