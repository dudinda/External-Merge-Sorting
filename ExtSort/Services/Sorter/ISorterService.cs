namespace ExtSort.Services.Sorter
{
    public interface ISorterService : IDisposable
    {
        Task SortFile(string srcFile, string dstFile, CancellationToken token);
    }
}
