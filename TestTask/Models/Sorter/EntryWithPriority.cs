namespace TestTask.Models.Sorter
{
    internal class EntryWithPriority
    {
        public Entry Item { get; init; }
        public (string Str, int Int) Priority { get; init; }
    }
}
