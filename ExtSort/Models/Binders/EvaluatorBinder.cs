using ExtSort.Services.Factories;

using System.CommandLine.Parsing;

namespace ExtSort.Models.Binders
{
    internal record EvaluatorBinder()
    {
        public EvaluatorBinder(ParseResult parser) : this()
        {
            var args = ArgumentFactory.EvaluatorArguments.Value;

            if (!int.TryParse(parser.GetValueForArgument(args[nameof(DiskLatencyMs)])?.ToString(), out var diskLatencyMs))
                throw new InvalidCastException("The size of RAM available is in incorrect format.");
            if (!int.TryParse(parser.GetValueForArgument(args[nameof(DiskRandomReadSpeedMbs)])?.ToString(), out var diskRandomReadSpeedMbs))
                throw new InvalidCastException("The size of RAM available is in incorrect format.");

            FileSizeMb = (int)parser.GetValueForArgument(args[nameof(FileSizeMb)]);
            RamAvailableMb = (int)parser.GetValueForArgument(args[nameof(RamAvailableMb)]);
            NumberOfFiles = (int)parser.GetValueForArgument(args[nameof(NumberOfFiles)]);

            DiskLatencyMs = diskLatencyMs;
            DiskRandomReadSpeedMbs = diskRandomReadSpeedMbs;
        }

        public int FileSizeMb { get; init; }
        public int RamAvailableMb { get; init; }
        public int NumberOfFiles { get; init; }
        public int DiskLatencyMs { get; init; }
        public int DiskRandomReadSpeedMbs { get; init; }
    }
}
