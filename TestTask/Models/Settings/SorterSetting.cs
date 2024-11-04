namespace TestTask.Models.Settings
{
    public class SorterSetting
    {
        public int FileSplitSizeKb { get; init; } = 1024 * 1024;
        public ReadWritePath IOPath { get; init; } = new ReadWritePath(); 
        public char NewLineSeparator { get; init; } = '\n';

        public int SortPageSize { get; init; } = 16;
        public int SortInputBufferSize { get; init; } = 65536 * 2;
        public int SortOutputBufferSize { get; init; } = 65536 * 2;
        public int SortThenMergePageSize { get; init; } = 16;
        public int SortThenMergeChunkSize { get; init; } = 16;

        public int MergePageSize { get; init; } = 16;
        public int MergeChunkSize { get; init; } = 16;
        public int MergeInputBufferSize { get; init; } = 65536 * 2;
        public int MergeOutputBufferSize { get; init; } = 65536 * 2;
    }
}
