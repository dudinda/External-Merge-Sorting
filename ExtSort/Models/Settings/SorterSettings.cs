using System.Text;

namespace ExtSort.Models.Settings 
{
    public record SorterSettings()
    {
        public SorterSettings(SorterSettings settings) 
        {
            NumberOfFiles = settings.NumberOfFiles;
            SortPageSize = settings.SortPageSize;
            SortOutputBufferSize = settings.SortOutputBufferSize;
            MergePageSize = settings.MergePageSize;
            MergeChunkSize = settings.MergeChunkSize;
            MergeOutputBufferSize = settings.MergeOutputBufferSize;
            IOPath = settings.IOPath;
            Format = settings.Format;
        }

        public int NumberOfFiles { get; init; } = 64;
        public int SortPageSize { get; init; } = 16;
        public int SortOutputBufferSize { get; init; } = 81920;
        public int MergePageSize { get; init; } = 4;
        public int MergeChunkSize { get; init; } = 4;
        public int MergeOutputBufferSize { get; init; } = 81920;
        public ReadWritePath IOPath { get; init; } = new ReadWritePath();
        public FormatSettings Format { get; init; } = new FormatSettings();

        public virtual bool Validate(out StringBuilder errors)
        {
            errors = new StringBuilder();
            if(NumberOfFiles <= 0)
                errors.AppendLine("The number of chunks be greater than 0");
            if (SortPageSize <= 0)
                errors.AppendLine("Sort page size must be greater than 0");
            if (!IOPath.Validate(out var ioErrors))
                errors.Append(ioErrors);
            if (!Format.Validate(out var formatErrors))
                errors.Append(formatErrors);

            return errors.Length == 0;
        }
    }
}
