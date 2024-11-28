using ExtSort.Code.Extensions;
using ExtSort.Models.Settings;
using ExtSort.Models.Sorter;
using ExtSort.Services.Sorter.Implementation;

namespace ExtSort.Services.Sorter
{
    public class SorterServiceCPUBound : ISorterService
    {
        private readonly SorterSetting _settings;
        private readonly SorterServiceIOBound _ioBound;
        internal const string _SortedFileExtension = ".sorted";
        private const string _TempFileExtension = ".tmp";

        public SorterServiceCPUBound(SorterSetting settings)
        {
            _settings = settings;
            _ioBound = new SorterServiceIOBound(settings);
        }

        public async Task SortFile(string srcFile, string dstFile, CancellationToken token)
        {
            Console.WriteLine("--Splitting/Sorting--");
            Console.WriteLine($"Page size: {_settings.SortPageSize}");
            await SplitFile(srcFile, _settings.FileSplitSizeKb, token);

            Console.WriteLine($"{Environment.NewLine}--Merging--");
            var sortedFiles = _ioBound.MoveTmpFilesToSorted(_settings.IOPath.MergeStartTargetPath);
            await _ioBound.MergeFiles(sortedFiles, dstFile, token);
        }

        private async Task SplitFile(
            string srcFile, long fileSizeKb, CancellationToken token)
        {
            var fileSize = fileSizeKb * 1024;
            var totalRead = fileSize;
            var srcPath = Path.Combine(_settings.IOPath.SplitReadPath, srcFile);
            if (!Directory.Exists(_settings.IOPath.SplitReadPath))
                throw new InvalidOperationException($"Directory {_settings.IOPath.SplitReadPath} does not exist.");
            Console.WriteLine($"Splitting {srcFile} into files of the {fileSizeKb / 1024:0.###} MB");
            await using (var sourceStream = File.OpenRead(srcPath))
            {
                using (var reader = new StreamReader(sourceStream))
                {
                    var file = 1;
                    var page = 0;
                    var tasks = new List<Task>();
                    while (!reader.EndOfStream && reader.BaseStream.Position <= totalRead && !token.IsCancellationRequested)
                    {
                        var queue = _ioBound.BuildQueue(750000); 
                        Console.Write($"\rCurrent file: {file}");
                        string line;
                        while ((line = reader.ReadLine()) != null &&
                                line.TryParsePriority(out var priority) &&
                                !token.IsCancellationRequested)
                        {
                            queue.Enqueue(new Entry() { Row = line }, priority);
                            if (reader.BaseStream.Position >= totalRead)
                                break;
                        }
                        if (string.IsNullOrEmpty(line))
                            break;
                        token.ThrowIfCancellationRequested();
                        totalRead = reader.BaseStream.Position + fileSize;
                        ++file;
                        tasks.Add(Task.Run(() =>
                        {
                            var fileName = $"{file}{_SortedFileExtension}{_TempFileExtension}";
                            using var writer = new StreamWriter(Path.Combine(_settings.IOPath.SortWritePath, fileName));
                            Entry row;
                            while (queue.TryDequeue(out row, out _) && !token.IsCancellationRequested)
                            {
                                writer.WriteLine(row.Row.AsMemory());
                            }
                            token.ThrowIfCancellationRequested();
                        }, token));
                        if (tasks.Count == _settings.SortPageSize)
                        {
                            ++page;
                            Console.WriteLine($"{Environment.NewLine}Waiting the {page} page to be sorted");
                            await Task.WhenAll(tasks);
                            tasks.Clear();
                        }
                    }
                }
                token.ThrowIfCancellationRequested();
            }
        }
    }
}
