namespace ExtSort.Models.Settings
{
    public record SorterIOSettings : SorterSettings
    {
        public int SortThenMergePageSize { get; init; } = 4;
        public int SortThenMergeChunkSize { get; init; } = 4;
    }
}
