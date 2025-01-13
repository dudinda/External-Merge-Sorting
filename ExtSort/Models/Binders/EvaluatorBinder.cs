using ExtSort.Services.Factories;

using System.CommandLine.Parsing;

namespace ExtSort.Models.Binders
{
    internal record EvaluatorBinder
    {
        public EvaluatorBinder(ParseResult parser) 
        {
            var args = ArgumentFactory.EvaluatorArguments.Value;
            var fileSize = parser.GetValueForArgument(args[nameof(FileSizeMb)]);
            var ramSize = parser.GetValueForArgument(args[nameof(RamAvailableMb)]);

            if (!int.TryParse(fileSize?.ToString(), out var fileSizeMb))
                throw new InvalidCastException("The size of a file is in incorrect format.");
            if (!int.TryParse(ramSize?.ToString(), out var ramSizeMb))
                throw new InvalidCastException("The size of RAM available is in incorrect format.");

            FileSizeMb = fileSizeMb;
            RamAvailableMb = ramSizeMb;
        }

        public int FileSizeMb { get; init; }
        public int RamAvailableMb { get; init; }
    }
}
