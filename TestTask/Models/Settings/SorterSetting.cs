using System.Text;

namespace TestTask.Models.Settings
{
    public class SorterSetting 
    {
        public int FileSplitSizeKb { get; init; } = 15625;
        public ReadWritePath IOPath { get; init; } = new ReadWritePath(); 
        public char NewLineSeparator { get; init; } = '\n';

        public int SortPageSize { get; init; } = 16;
        public int SortInputBufferSize { get; init; } = 4096;
        public int SortOutputBufferSize { get; init; } = 81920;
        public int SortThenMergePageSize { get; init; } = 4;
        public int SortThenMergeChunkSize { get; init; } = 4;

        public int MergePageSize { get; init; } = 4;
        public int MergeChunkSize { get; init; } = 4;
        public int MergeInputBufferSize { get; init; } = 4096;
        public int MergeOutputBufferSize { get; init; } = 81920;

        public bool Validate(out StringBuilder errors)
        {
            errors = new StringBuilder();
            if(FileSplitSizeKb <= 0)
                errors.AppendLine("Chunk size must be greater than 0 (kb)");
            if (SortPageSize <= 0)
                errors.AppendLine("Sort page size must be greater than 0");
            if (!IOPath.Validate(out var ioerrors))
                errors.Append(ioerrors);

            return errors.Length == 0;
        }
    }
}
