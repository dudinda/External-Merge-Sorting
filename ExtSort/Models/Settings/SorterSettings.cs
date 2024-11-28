using System.Text;

namespace ExtSort.Models.Settings
{
    public class SorterSettings 
    {
        public int NumberOfFiles { get; init; } = 64;
        public ReadWritePath IOPath { get; init; } = new ReadWritePath(); 
        public char NewLineSeparator { get; init; } = '\n';

        public int SortPageSize { get; init; } = 16;
        public int SortOutputBufferSize { get; init; } = 81920;
        public int MergePageSize { get; init; } = 4;
        public int MergeChunkSize { get; init; } = 4;
        public int MergeOutputBufferSize { get; init; } = 81920;

        public virtual bool Validate(out StringBuilder errors)
        {
            errors = new StringBuilder();
            if(NumberOfFiles <= 0)
                errors.AppendLine("Chunk size must be greater than 0 (kb)");
            if (SortPageSize <= 0)
                errors.AppendLine("Sort page size must be greater than 0");
            if (!IOPath.Validate(out var ioerrors))
                errors.Append(ioerrors);

            return errors.Length == 0;
        }
    }
}
