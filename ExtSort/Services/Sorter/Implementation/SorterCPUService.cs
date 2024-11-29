using ExtSort.Code.Extensions;
using ExtSort.Models.Settings;
using ExtSort.Services.Sorter.Implementation;

using System.Text;

namespace ExtSort.Services.Sorter
{
    public class SorterCPUService : ISorterService
    {
        private const string _SortedFileExtension = SorterIOService._SortedFileExtension;
        private const string _TempFileExtension = SorterIOService._TempFileExtension;

        private readonly SorterCPUSettings _settings;
        private readonly SorterIOService _io;

        public SorterCPUService(SorterCPUSettings settings)
        {
            _settings = settings ?? throw new NullReferenceException(nameof(settings));
            _io = new SorterIOService(new SorterIOSettings()
            {
                NumberOfFiles = settings.NumberOfFiles,
                IOPath = settings.IOPath,
                NewLineSeparator = settings.NewLineSeparator,
                SortPageSize = settings.SortPageSize,
                SortOutputBufferSize = settings.SortOutputBufferSize,
                MergePageSize = settings.MergePageSize,
                MergeChunkSize = settings.MergeChunkSize,
                MergeOutputBufferSize = settings.MergeOutputBufferSize,
            });
        }

        public async Task SortFile(string srcFile, string dstFile, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(srcFile))
                throw new InvalidOperationException("The name of a source file cannot be empty");
            if (string.IsNullOrWhiteSpace(dstFile))
                throw new InvalidOperationException("The name of a destination file cannot be empty");

            Console.WriteLine("--Splitting/Sorting--");
            Console.WriteLine($"Page size: {_settings.SortPageSize}");
            await SplitFile(srcFile, _settings.NumberOfFiles, token);

            Console.WriteLine($"{Environment.NewLine}--Merging--");
            var sortedFiles = _io.MoveTmpFilesToSorted(_settings.IOPath.MergeStartTargetPath);
            await _io.MergeFiles(sortedFiles, dstFile, token);
        }

        private async Task SplitFile(string srcFile, long numberOfFiles, CancellationToken token)
        {
            var srcPath = Path.Combine(_settings.IOPath.SplitReadPath, srcFile);
            if (!Directory.Exists(_settings.IOPath.SplitReadPath))
                throw new InvalidOperationException($"Directory {_settings.IOPath.SplitReadPath} does not exist.");
            await using (var sourceStream = File.OpenRead(srcPath))
            {
                using (var reader = new StreamReader(sourceStream))
                {
                    var fileSize = sourceStream.Length / numberOfFiles;
                    var totalRead = 0l;
                    var file = 0;
                    var page = 0;
                    var tasks = new List<Task>();
                    Console.WriteLine($"Splitting {srcFile} into files of the {fileSize / (1024 * 1024):0.###} MB");
                    while (!reader.EndOfStream && sourceStream.Length > totalRead && !token.IsCancellationRequested)
                    {
                        var queue = _io.BuildQueue<string>(_settings.BufferCapacityLines);
                        Console.Write($"\rCurrent file: {++file}");
                        string line;
                        while ((line = reader.ReadLine()) != null &&
                                line.TryParsePriority(out var priority) &&
                                !token.IsCancellationRequested)
                        {
                            queue.Enqueue(null, priority);
                            if (sourceStream.Position >= totalRead)
                                break;
                        }
                        token.ThrowIfCancellationRequested();
                        totalRead = sourceStream.Position + fileSize;
                        var fileName = $"{file}{_SortedFileExtension}{_TempFileExtension}";
                        
                        tasks.Add(Task.Run(() =>
                        {
                            using (var stream = File.OpenWrite(Path.Combine(_settings.IOPath.SortWritePath, fileName)))
                            {
                                stream.SetLength(fileSize);
                                using (var writer = new StreamWriter(stream, bufferSize: _settings.SortOutputBufferSize))
                                {
                                    var builder = new StringBuilder(); (string Str, int Int) row;
                                    while (queue.TryDequeue(out _, out row) && !token.IsCancellationRequested)
                                    {
                                        writer.WriteLine(builder.Append(row.Int).Append('.').Append(row.Str));
                                        builder.Clear();
                                    }

                                    token.ThrowIfCancellationRequested();
                                }
                            }
                        }, token));

                        if (tasks.Count == _settings.SortPageSize)
                        {
                            Console.WriteLine($"{Environment.NewLine}Waiting the {++page} page to be sorted");
                            await Task.WhenAll(tasks);
                            tasks.Clear();
                        }
                    }
                    if (tasks.Any())
                        await Task.WhenAll(tasks);
                    tasks.Clear();
                }
                token.ThrowIfCancellationRequested();
            }
        }

        public void Dispose()
        {
            _io.Dispose();
        }
    }
}
