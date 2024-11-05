using System.Collections.Concurrent;
using System.Text;

using TestTask.Code.Comparers;
using TestTask.Code.Extensions;
using TestTask.Models.Settings;
using TestTask.Models.Sorter;

namespace TestTask.Services.Sorter
{
    public class SorterService 
    {
        private const string _UnsortedFileExtension = ".unsorted";
        private const string _SortedFileExtension = ".sorted";
        private const string _TempFileExtension = ".tmp";
        private const int _EOF = -1;
        private const int _NULL = 0;

        private readonly SorterSetting _settings;

        private int _mergeTempCounter = 0;
        private Dictionary<string, int> _fileToNumberOfLines = new();

        public SorterService(SorterSetting settings)
        {
            _settings = settings;
        }

        public async Task SortFile(string srcFile, string dstFile, CancellationToken token)
        {
            Console.WriteLine("--Splitting--");
            var files = await SplitFile(srcFile, _settings.FileSplitSizeKb, token);

            Console.WriteLine($"{Environment.NewLine}--Sorting/Merging--");
            var mergeTasks = await SortFiles(files, token);
            await Task.WhenAll(mergeTasks);

            Console.WriteLine($"{Environment.NewLine}--Merging--");
            var sortedFiles = MoveTmpFilesToSorted(_settings.IOPath.MergeStartTargetPath);
            await MergeFiles(sortedFiles, dstFile, token);
        }

        private async Task<IReadOnlyCollection<string>> SplitFile(
            string srcFile, long fileSizeKb, CancellationToken token)
        {
            var fileSize = fileSizeKb * 1024;
            var buffer = new byte[fileSize];
            var extraBuffer = new List<byte>();
            var srcPath = Path.Combine(_settings.IOPath.SplitReadPath, srcFile);
            if (!Directory.Exists(_settings.IOPath.SortReadPath))
                throw new InvalidOperationException($"Directory {_settings.IOPath.SortReadPath} does not exist.");
            Console.WriteLine($"Splitting {srcFile} into files of the {fileSizeKb / 1024:0.###} MB");
            await using (var sourceStream = File.OpenRead(srcPath))
            {
                var currentFile = 0;
                while (sourceStream.Position < sourceStream.Length && !token.IsCancellationRequested)
                {
                    var totalRows = 0;
                    var runBytesRead = 0;
                    while (runBytesRead < fileSize && !token.IsCancellationRequested)
                    {
                        var value = sourceStream.ReadByte();
                        if (value == _EOF || value == _NULL)
                        {
                            break;
                        }

                        var @byte = (byte)value;
                        buffer[runBytesRead] = @byte;
                        ++runBytesRead;
                        if (@byte == _settings.NewLineSeparator)
                        {
                            ++totalRows;
                        }
                    }
                    token.ThrowIfCancellationRequested();

                    var extraByte = buffer[fileSize - 1];

                    while (extraByte != _settings.NewLineSeparator)
                    {
                        var flag = sourceStream.ReadByte();
                        if (flag == _EOF || flag == _NULL)
                        {
                            break;
                        }
                        extraByte = (byte)flag;
                        extraBuffer.Add(extraByte);
                    }

                    var filename = $"{++currentFile}{_UnsortedFileExtension}";
                    await using var unsortedFile = File.Create(Path.Combine(_settings.IOPath.SortReadPath, filename));
                    unsortedFile.SetLength(runBytesRead + extraBuffer.Count);
                    await unsortedFile.WriteAsync(buffer, 0, runBytesRead, token);
                    if (extraBuffer.Count > 0)
                    {
                        ++totalRows;
                        await unsortedFile.WriteAsync(extraBuffer.ToArray(), 0, extraBuffer.Count, token);
                    }

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
            var mergeTargetLocation = _settings.IOPath.MergeStartTargetPath;
            var pageStepMerge = _settings.SortThenMergePageSize;
            var chunkSize = _settings.SortThenMergeChunkSize;

            var sortedTasks = new List<Task>();
            var sorted = new ConcurrentBag<string>();
            var total = unsortedFiles.Count;
            var pageStep = _settings.SortPageSize;

            if(pageStepMerge * chunkSize < pageStep)
            {
                var sqrt = (decimal)Math.Sqrt(pageStep);
                pageStepMerge = (int)Math.Ceiling(sqrt);
                chunkSize = (int)Math.Floor(sqrt);
            }

            var page = 0;
            var start = 0;
            do
            {
                var iterator = unsortedFiles.Skip(start).Take(pageStep);
                if (iterator.Any())
                {
                    Console.WriteLine($"Page: {page + 1}");
                    var digits = iterator.Select(file => Path.GetFileNameWithoutExtension(file));
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

                    var mergeTask = KWayMerge(new List<string>(sorted), mergeTargetLocation, mergeSourceLocation, chunkSize, 0, pageStepMerge, token);
                    mergeTasks.Add(mergeTask);

                    sorted.Clear();
                }
                start = pageStep * ++page;
            } while (start <= total);
            return mergeTasks;
        }

        private void SortFile(string unsortedFilePath, string sortedFilePath, int numberOfLines, CancellationToken token)
        {
            long targetSize = 0;
            var buffer = new (string Str, int Int)[numberOfLines];
            using (var unsorted = File.OpenRead(unsortedFilePath))
            {
                using var buffered = new BufferedStream(unsorted);
                using var streamReader = new StreamReader(buffered);
                var index = 0;
                while (!streamReader.EndOfStream && !token.IsCancellationRequested)
                {
                    var value = streamReader.ReadLine();
                    if (value.TryParseLine(out var row))
                    {
                        buffer[index] = row.Priority;
                        ++index;
                    }
                }
                targetSize = streamReader.BaseStream.Length;
            }
            token.ThrowIfCancellationRequested();

            buffer.SortWith((x, y) => x.Str.CompareTo(y.Str), (x, y) => x.Int.CompareTo(y.Int));

            using (var sorted = File.OpenWrite(sortedFilePath))
            {
                sorted.SetLength(targetSize);
                using (var streamWriter = new StreamWriter(sorted, bufferSize: _settings.SortOutputBufferSize))
                {
                    var builder = new StringBuilder();
                    foreach (var row in buffer)
                    {
                        builder.Append(row.Int).Append(".").Append(row.Str);
                        streamWriter.WriteLine(builder);
                        builder.Clear();
                        token.ThrowIfCancellationRequested();
                    }
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
        }
        
        private async Task MergeFiles(IReadOnlyList<string> sortedFiles, string targetName, CancellationToken token)
        {
            var mergeSourceLocation = _settings.IOPath.MergeStartPath;
            var mergeTargetLocation = _settings.IOPath.MergeStartTargetPath;
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
                    Console.WriteLine($"Page: {page + 1}");
                    await KWayMerge(sortedFiles, mergeTargetLocation, mergeSourceLocation, chunkSize, step, pageStep, token);
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
            var outputPath = Path.Combine(mergeSourceLocation, targetName);
            File.Move(resultPath, outputPath, true);
        }

        private async Task KWayMerge(IReadOnlyList<string> sortedFiles,
            string mergeTargetLocation, string mergeSourceLocation,
            int chunkSize, int step, int pageStep, CancellationToken token)
        {
            var tasks = new List<Task>();
            foreach (var chunk in sortedFiles.Chunk(chunkSize).Skip(step).Take(pageStep))
            {
                var counter = Interlocked.Increment(ref _mergeTempCounter);
                var outputFilename = $"{counter}{_SortedFileExtension}{_TempFileExtension}";
                var targetPath = Path.Combine(mergeTargetLocation, outputFilename);
                var digits = chunk.Select(file => Path.GetFileNameWithoutExtension(file));
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

            while (!token.IsCancellationRequested && finishedStreamReaders.Count != streamReaders.Length)
            {
                var entry = priorityQueue.Dequeue();
                var streamReaderIndex = entry.StreamReaderIdx;
                outputWriter.WriteLine(entry.Row.AsMemory());

                var value = streamReaders[streamReaderIndex].ReadLine();
                if(value.TryParseLine(out var entryWithPriority))
                {
                    entryWithPriority.Item.StreamReaderIdx = streamReaderIndex;
                    priorityQueue.Enqueue(entryWithPriority.Item, entryWithPriority.Priority);
                    continue;
                }

                if (streamReaders[streamReaderIndex].EndOfStream)
                {
                    finishedStreamReaders.Add(streamReaderIndex);
                }
            }

            token.ThrowIfCancellationRequested();
            Clean(streamReaders, filesToMerge, readerSourcePath);
        }

        private StreamReader[]  InitKWayMergeFromStreams(
            IReadOnlyList<string> sortedFiles,
            string readerSourcePath,
            out PriorityQueue<Entry, (string, int)>  queue)
        {
            var streamReaders = new StreamReader[sortedFiles.Count];
            var comparisons = new Comparison<(string Str, int Int)>[]
            {
                (x, y) => x.Str.CompareTo(y.Str),
                (x, y) => x.Int.CompareTo(y.Int)
            };
            var comparer = new TaskTemplateComparer<(string Str, int Int)>(comparisons);
            queue = new PriorityQueue<Entry, (string, int)>(sortedFiles.Count, comparer);
            for (var i = 0; i < sortedFiles.Count; i++)
            {
                var sortedFilePath = Path.Combine(readerSourcePath, sortedFiles[i]);
                var sortedFileStream = File.OpenRead(sortedFilePath);
                var buffered = new BufferedStream(sortedFileStream);
                streamReaders[i] = new StreamReader(sortedFileStream);
                var value = streamReaders[i].ReadLine();
                if(value.TryParseLine(out var entry))
                {
                    entry.Item.StreamReaderIdx = i;
                    queue.Enqueue(entry.Item, entry.Priority);
                }
            }

            return streamReaders;
        }

        private void Clean(StreamReader[] streamReaders,
            IReadOnlyList<string> filesToMerge,
            string cleanLocation)
        {
            for (var i = 0; i < streamReaders.Length; i++)
            {
                streamReaders[i].Dispose();

                var temporaryFilename = $"{filesToMerge[i]}.removal";
                var tmpPath = Path.Combine(cleanLocation, temporaryFilename);
                var filePath = Path.Combine(cleanLocation, filesToMerge[i]);
                File.Move(filePath, tmpPath, true);
                File.Delete(tmpPath);
            }
        }

        private IReadOnlyList<string> MoveTmpFilesToSorted(string tmpPath)
        {
            var sortedFilesTmp = Directory.GetFiles(tmpPath, $"*{_SortedFileExtension}{_TempFileExtension}");
            var sortedFiles = new List<string>();
            foreach (var file in sortedFilesTmp)
            {
                var sorted = file.Replace(_TempFileExtension, string.Empty);
                File.Move(file, sorted, true);
                sortedFiles.Add(sorted);
            }

            return sortedFiles;
        }
    }
}
