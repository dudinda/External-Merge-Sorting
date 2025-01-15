using ExtSort.Services.Factories;

using System.CommandLine.Parsing;

namespace ExtSort.Models.Binders
{
    internal record EvaluatorBinder()
    {
        public EvaluatorBinder(ParseResult parser) : this()
        {
            var args = ArgumentFactory.EvaluatorArguments.Value;

            if (!int.TryParse(parser.GetValueForArgument(args[nameof(FileSizeMb)])?.ToString(), out var fileSizeMb))
                throw new InvalidCastException("The size of a file is in incorrect format.");
            if (!int.TryParse(parser.GetValueForArgument(args[nameof(RamAvailableMb)])?.ToString(), out var ramSizeMb))
                throw new InvalidCastException("The size of RAM available is in incorrect format.");
            if (!int.TryParse(parser.GetValueForArgument(args[nameof(DiskLatencyMs)])?.ToString(), out var diskLatencyMs))
                throw new InvalidCastException("The size of RAM available is in incorrect format.");
            if (!int.TryParse(parser.GetValueForArgument(args[nameof(DiskRandomReadSpeedMbs)])?.ToString(), out var diskRandomReadSpeedMbs))
                throw new InvalidCastException("The size of RAM available is in incorrect format.");
            if (!int.TryParse(parser.GetValueForArgument(args[nameof(NumberOfFiles)])?.ToString(), out var numberOfFiles))
                throw new InvalidCastException("The size of RAM available is in incorrect format.");

            FileSizeMb = fileSizeMb;
            RamAvailableMb = ramSizeMb;
            DiskLatencyMs = diskLatencyMs;
            DiskRandomReadSpeedMbs = diskRandomReadSpeedMbs;
            NumberOfFiles = numberOfFiles;
        }

        public int FileSizeMb { get; init; }
        public int RamAvailableMb { get; init; }
        public int NumberOfFiles { get; init; }
        public int DiskLatencyMs { get; init; }
        public int DiskRandomReadSpeedMbs { get; init; }
    }
}
