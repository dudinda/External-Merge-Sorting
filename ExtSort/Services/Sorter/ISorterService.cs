namespace ExtSort.Services.Sorter
{
    public interface ISorterService
    {
        Task SortFile(string srcFile, string dstFile, CancellationToken token);
    }
}
