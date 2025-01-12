using ExtSort.Code.Comparers;
using ExtSort.Code.Extensions;
using ExtSort.Models.Settings;
using ExtSort.Models.Sorter;

using System.Collections.Concurrent;
using System.Numerics;
using System.Text;

namespace ExtSort.Services.Sorter.Implementation 
{
    public class SorterIOService : ISorterService
    {
        internal const string _SortedFileExtension = ".sorted";
        internal const string _TempFileExtension = ".tmp";

        private const string _UnsortedFileExtension = ".unsorted";
        private const int _EOF = -1;
        private const int _NULL = 0;

        private readonly SorterIOSettings _settings;

        private int _mergeTempCounter = 0;
        private Dictionary<string, int> _fileToNumberOfLines = new();

        public SorterIOService(SorterIOSettings settings)
        {
            _settings = settings ?? throw new NullReferenceException(nameof(settings));
        }

        public async Task SortFile(string srcFile, string dstFile, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(srcFile))
                throw new InvalidOperationException("The name of a source file cannot be empty");
            if (string.IsNullOrWhiteSpace(dstFile))
                throw new InvalidOperationException("The name of a destination file cannot be empty");

            Console.WriteLine("--Splitting--");
            var files = await SplitFile(srcFile, _settings.NumberOfFiles, token);

            Console.WriteLine($"{Environment.NewLine}--Sorting/Merging--");
            var mergeTasks = await SortFiles(files, token);
            await Task.WhenAll(mergeTasks);

            Console.WriteLine($"{Environment.NewLine}--Merging--");
            var sortedFiles = MoveTmpFilesToSorted(_settings.IOPath.MergeStartPath);
            var source = _settings.IOPath.MergeStartPath;
            var target = _settings.IOPath.MergeStartTargetPath;
            await MergeFiles(sortedFiles, dstFile, source, target, token);
        }

        private async Task<IReadOnlyCollection<string>> SplitFile(
            string srcFile, long numberOfFiles, CancellationToken token)
        {
            var srcPath = Path.Combine(_settings.IOPath.SplitReadPath, srcFile);
            if (!Directory.Exists(_settings.IOPath.SplitReadPath))
                throw new InvalidOperationException($"Directory {_settings.IOPath.SplitReadPath} does not exist.");
            await using (var sourceStream = File.OpenRead(srcPath))
            {
                var fileSize = sourceStream.Length / numberOfFiles;
                var totalRead = 0l;
                var currentFile = 0;
                var tasks = new List<Task>();
                var buffer = new byte[fileSize];
                var extraBuffer = new List<byte>();
                var newLine = _settings.Format.NewLineDelimiter;
                Console.WriteLine($"Splitting {srcFile} into files of the {fileSize / (1024 * 1024):0.###} MB");
                while (sourceStream.Length > totalRead && !token.IsCancellationRequested)
                {
                    var filename = $"{++currentFile}{_UnsortedFileExtension}";
                    Console.Write($"\rCurrent file: {filename}");

                    var totalRows = 0;
                    var runBytesRead = 0;
                    while (runBytesRead < fileSize && !token.IsCancellationRequested)
                    {
                        var value = sourceStream.ReadByte();
                        if (value == _EOF || value == _NULL)
                            break;

                        var @byte = (byte)value;
                        buffer[runBytesRead] = @byte;
                        ++runBytesRead;
                        if (@byte == newLine)
                            ++totalRows;
                    }
                    token.ThrowIfCancellationRequested();

                    var extraByte = buffer[fileSize - 1];

                    while (extraByte != newLine)
                    {
                        var flag = sourceStream.ReadByte();
                        if (flag == _EOF || flag == _NULL)
                            break;
                        extraByte = (byte)flag;
                        extraBuffer.Add(extraByte);
                    }

                    await using var unsortedFile = File.Create(Path.Combine(_settings.IOPath.SortReadPath, filename));
                    var targetSize = runBytesRead + extraBuffer.Count;
                    unsortedFile.SetLength(targetSize);
                    await unsortedFile.WriteAsync(buffer, 0, runBytesRead, token);
                    if (extraBuffer.Count > 0)
                    {
                        ++totalRows;
                        unsortedFile.Write(extraBuffer.ToArray(), 0, extraBuffer.Count);
                    }
                    totalRead += targetSize;
                    _fileToNumberOfLines.Add(filename, totalRows);
                    Array.Clear(buffer, 0, runBytesRead);
                    extraBuffer.Clear();
                }
                token.ThrowIfCancellationRequested();
                return _fileToNumberOfLines.Keys;
            }
        }

        private async Task<IReadOnlyList<Task>> SortFiles(
            IReadOnlyCollection<string> unsortedFiles, CancellationToken token)
        {
            var mergeTasks = new List<Task>();
            var mergeSourceLocation = _settings.IOPath.SortWritePath;
            var mergeTargetLocation = _settings.IOPath.MergeStartPath;
            var pageStepMerge = _settings.SortThenMergePageSize;
            var chunkSize = _settings.SortThenMergeChunkSize;

            var sortedTasks = new List<Task>();
            var sorted = new ConcurrentBag<string>();
            var total = unsortedFiles.Count;
            var pageStep = _settings.SortPageSize;

            if (pageStepMerge * chunkSize < pageStep)
            {
                var sqrt = (decimal)Math.Sqrt(pageStep);
                pageStepMerge = (int)Math.Ceiling(sqrt);
                chunkSize = (int)Math.Floor(sqrt);
            }

            var page = 0;
            var start = 0;
            do
            {
                var iterator = unsortedFiles.Skip(start).Take(pageStep).ToArray();
                if (iterator.Any())
                {
                    Console.WriteLine($"Page: {page + 1}");
                    var digits = iterator.Select(file => Path.GetFileNameWithoutExtension(file)).ToArray();
                    Console.WriteLine($"Sorting: [{string.Join(", ", digits)}]{_UnsortedFileExtension}");
                    foreach (var file in iterator)
                    {
                        sortedTasks.Add(Task.Run(() =>
                        {
                            var sortedFilename = file.Replace(_UnsortedFileExtension, _SortedFileExtension);
                            var unsortedFilePath = Path.Combine(_settings.IOPath.SortReadPath, file);
                            var sortedFilePath = Path.Combine(_settings.IOPath.SortWritePath, sortedFilename);
                            SortFile(unsortedFilePath, sortedFilePath, _fileToNumberOfLines[file], token);
                            File.Delete(unsortedFilePath);
                            sorted.Add(sortedFilename);
                        }, token));
                    }
                    await Task.WhenAll(sortedTasks);

                    var sortedPage = sorted.ToList().Chunk(chunkSize).Skip(0).Take(pageStepMerge).ToArray();
                    var mergeTask = KWayMerge(sortedPage, mergeTargetLocation, mergeSourceLocation, token);
                    mergeTasks.Add(mergeTask);

                    sorted.Clear();
                    sortedTasks.Clear();
                }
                start = pageStep * ++page;
            } while (start <= total);
            return mergeTasks;
        }

        private void SortFile(string unsortedFilePath, string sortedFilePath, int numberOfLines, CancellationToken token)
        {
            long targetSize = 0;
            var buffer = new (string Str, BigInteger Int)[numberOfLines];
            using (var unsorted = File.OpenRead(unsortedFilePath))
            {
                using var buffered = new BufferedStream(unsorted);
                using var streamReader = new StreamReader(buffered);
                var index = 0;
                while (!streamReader.EndOfStream && !token.IsCancellationRequested)
                {
                    var value = streamReader.ReadLine();
                    if (value.TryParsePriority(out var priority))
                    {
                        buffer[index] = priority;
                        ++index;
                    }
                }
                targetSize = streamReader.BaseStream.Length;
                token.ThrowIfCancellationRequested();
            }

            try
            {
                buffer.SortWith(token,
                    (x, y) => x.Str.AsSpan().CompareTo(y.Str.AsSpan(), StringComparison.Ordinal),
                    (x, y) => x.Int.CompareTo(y.Int));
            }
            catch (Exception ex) when (ex.InnerException is OperationCanceledException) { throw ex.InnerException; }

            using (var sorted = File.OpenWrite(sortedFilePath))
            {
                sorted.SetLength(targetSize);
                using (var streamWriter = new StreamWriter(sorted, bufferSize: _settings.SortOutputBufferSize))
                {
                    var builder = new StringBuilder();
                    var index = 0;
                    (string Str, BigInteger Int) row;
                    var separator = _settings.Format.ColumnSeparator;
                    while (index < buffer.Length && !token.IsCancellationRequested)
                    {
                        row = buffer[index];
                        builder.Append(row.Int).Append(separator).Append(row.Str);
                        streamWriter.WriteLine(builder);
                        builder.Clear();
                        ++index;
                    }
                    Array.Clear(buffer, 0, buffer.Length);
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        public async Task MergeFiles(
            IReadOnlyList<string> sortedFiles,
            string targetName,
            string mergeSourceLocation,
            string mergeTargetLocation,
            CancellationToken token)
        {
            var iteration = 1;
            while (sortedFiles.Count > 1 && !token.IsCancellationRequested)
            {
                Console.WriteLine($"Iteration: {iteration}");
                Console.WriteLine($"Merging files from {mergeSourceLocation} into {mergeTargetLocation}");
                var pageStep = _settings.MergePageSize;
                var chunkSize = _settings.MergeChunkSize;
                var total = sortedFiles.Count;

                var page = 0;
                var start = 0;
                var step = 0;
                _mergeTempCounter = 0;
                do
                {
                    var sortedPage = sortedFiles.Chunk(chunkSize).Skip(step).Take(pageStep).ToArray();
                    if (!sortedPage.Any()) break;
                    Console.WriteLine($"Page: {page + 1}");
                    await KWayMerge(sortedPage, mergeTargetLocation, mergeSourceLocation, token);
                    step = ++page * pageStep;
                    start = step * chunkSize;
                } while (start <= total);

                sortedFiles = MoveTmpFilesToSorted(mergeTargetLocation);

                var tmp = mergeTargetLocation;
                mergeTargetLocation = mergeSourceLocation;
                mergeSourceLocation = tmp;
                ++iteration;
                Console.WriteLine();
            }

            token.ThrowIfCancellationRequested();
            var resultPath = Path.Combine(mergeSourceLocation, sortedFiles[0]);
            File.Move(resultPath, targetName, true);
        }

        private async Task KWayMerge(IEnumerable<string[]> sortedChunks,
            string mergeTargetLocation, string mergeSourceLocation, CancellationToken token)
        {
            var tasks = new List<Task>();
            foreach (var chunk in sortedChunks)
            {
                var counter = Interlocked.Increment(ref _mergeTempCounter);
                var outputFilename = $"{counter}{_SortedFileExtension}{_TempFileExtension}";
                var targetPath = Path.Combine(mergeTargetLocation, outputFilename);
                var digits = chunk.Select(file => Path.GetFileNameWithoutExtension(file)).ToArray();
                Console.WriteLine($"Merging [{string.Join(", ", digits)}]{_SortedFileExtension} into: {outputFilename}");

                if (chunk.Length > 1)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var targetPath = Path.Combine(mergeTargetLocation, outputFilename);
                        return KWayMergeImpl(chunk, mergeSourceLocation, targetPath, token);
                    }, token));
                    continue;
                }
                var sourceFile = Path.Combine(mergeSourceLocation, chunk[0]);
                File.Move(sourceFile, targetPath, true);
            }
            await Task.WhenAll(tasks);
        }

        private async Task KWayMergeImpl(
            IReadOnlyList<string> filesToMerge,
            string readerSourcePath,
            string outputFilename,
            CancellationToken token)
        {
            var streamReaders = InitKWayMergeFromStreams(filesToMerge, readerSourcePath, out var priorityQueue);
            var finishedStreamReaders = new HashSet<int>();
            long targetSize = streamReaders.Sum(reader => reader.BaseStream.Length);

            using var outputStream = File.OpenWrite(outputFilename);
            await using var outputWriter = new StreamWriter(outputStream, bufferSize: _settings.SortOutputBufferSize);
            outputWriter.BaseStream.SetLength(targetSize);

            try
            {
                while (!token.IsCancellationRequested && finishedStreamReaders.Count != streamReaders.Length)
                {
                    var entry = priorityQueue.Dequeue();
                    var streamReaderIndex = entry.Index;
                    outputWriter.WriteLine(entry.Row.AsMemory());

                    var value = streamReaders[streamReaderIndex].ReadLine();
                    if (value.TryParsePriority(out var priority))
                    {
                        Entry row = new (value, streamReaderIndex);
                        priorityQueue.Enqueue(row, priority);
                        continue;
                    }

                    if (streamReaders[streamReaderIndex].EndOfStream)
                        finishedStreamReaders.Add(streamReaderIndex);
                }

                token.ThrowIfCancellationRequested();
            }
            finally
            {
                for (var i = 0; i < streamReaders.Length; i++)
                {
                    streamReaders[i].Dispose();
                    var filePath = Path.Combine(readerSourcePath, filesToMerge[i]);
                    Clean(filePath, readerSourcePath);
                }
            }
        }

        private StreamReader[] InitKWayMergeFromStreams(
            IReadOnlyList<string> sortedFiles,
            string readerSourcePath,
            out PriorityQueue<Entry, (string, BigInteger)> queue)
        {
            var streamReaders = new StreamReader[sortedFiles.Count];
            queue = BuildQueue<Entry>(sortedFiles.Count); 
            for (var i = 0; i < sortedFiles.Count; i++)
            {
                var sortedFilePath = Path.Combine(readerSourcePath, sortedFiles[i]);
                var sortedFileStream = File.OpenRead(sortedFilePath);
                var buffered = new BufferedStream(sortedFileStream);
                streamReaders[i] = new StreamReader(buffered);
                var value = streamReaders[i].ReadLine();
                if (value.TryParsePriority(out var priority))
                {
                    Entry row = new(value, i);
                    queue.Enqueue(row, priority);
                }
            }

            return streamReaders;
        }

        private void Clean(string filePath, string cleanLocation)
        {
            var temporaryFilename = $"{new FileInfo(filePath).Name}.removal";
            var tmpPath = Path.Combine(cleanLocation, temporaryFilename);
            File.Move(filePath, tmpPath, true);
            File.Delete(tmpPath);
        }

        public IReadOnlyList<string> MoveTmpFilesToSorted(string tmpPath)
        {
            var sortedFilesTmp = Directory.GetFiles(tmpPath, $"*{_SortedFileExtension}{_TempFileExtension}");
            var sortedFiles = new List<string>();
            foreach (var file in sortedFilesTmp)
            {
                var sorted = file.Replace(_TempFileExtension, string.Empty);
                File.Move(file, sorted, true);
                sortedFiles.Add(Path.GetFileName(sorted));
            }

            return sortedFiles;
        }

        internal PriorityQueue<T, (string, BigInteger)> BuildQueue<T>(int capacity)
        {
            var comparisons = new Comparison<(string Str, BigInteger Int)>[]
            {
                (x, y) => x.Str.AsSpan().CompareTo(y.Str.AsSpan(), StringComparison.Ordinal),
                (x, y) => x.Int.CompareTo(y.Int)
            };
            var comparer = new MultiColumnComparer<(string Str, BigInteger Int)>(comparisons);
            return new PriorityQueue<T, (string, BigInteger)>(capacity, comparer);
        }

        public void Dispose()
        {
            var files = Directory.GetFiles(_settings.IOPath.SortWritePath, "*.*", SearchOption.AllDirectories)
            .Where(s => 
                s.EndsWith(_TempFileExtension)   || 
                s.EndsWith(_SortedFileExtension) ||
                s.EndsWith(_UnsortedFileExtension));

            foreach (var file in files)
                Clean(file, _settings.IOPath.MergeStartTargetPath);
        }
    }
}
