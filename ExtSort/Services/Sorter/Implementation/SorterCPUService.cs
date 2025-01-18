using ExtSort.Code.Extensions;
using ExtSort.Code.Streams;
using ExtSort.Models.Settings;
using ExtSort.Models.Timer;
using ExtSort.Services.Sorter.Implementation;

using System.Numerics;
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
            _io = new SorterIOService(new SorterIOSettings(settings));
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
            var sortedFiles = _io.MoveTmpFilesToSorted(_settings.IOPath.SortWritePath);
            var source = _settings.IOPath.SortWritePath;
            var target = _settings.IOPath.MergeStartTargetPath;
            await _io.MergeFiles(sortedFiles, dstFile, source, target, token);
        }

        private async Task SplitFile(string srcFile, long numberOfFiles, CancellationToken token)
        {
            var srcPath = Path.Combine(_settings.IOPath.SplitReadPath, srcFile);
            if (!Directory.Exists(_settings.IOPath.SplitReadPath))
                throw new InvalidOperationException($"Directory {_settings.IOPath.SplitReadPath} does not exist.");
            var encoding = Encoding.GetEncoding(_settings.Format.EncodingName);
            await using (var sourceStream = File.OpenRead(srcPath))
            {
                using (var reader = new StreamReaderWrapper(sourceStream, encoding, false, 4096))
                {
                    var fileSize = Math.Max(1, sourceStream.Length / numberOfFiles);
                    var separator = _settings.Format.ColumnSeparator;
                    var totalRead = 0l;
                    var file = 0;
                    var page = 0;
                    var lineNumber = 0L;
                    var tasks = new List<Task>();

                    using var msgScope = new IntervalScope(1);
                    const string rowBokenFormatMessage = "Line number {0} contains content with a broken format: \"{1}\"";
                    const string fileMessage = "Current file: {0}";
                    Console.WriteLine($"Splitting {srcFile} into files of the {fileSize / (1024 * 1024):0.###} MB");

                    while (!reader.EndOfStream && !token.IsCancellationRequested)
                    {
                        var queue = _io.BuildQueue<string>(_settings.BufferCapacityLines);
                        ++file;
                        msgScope.WriteLine(string.Format(fileMessage, file));

                        while (!token.IsCancellationRequested)
                        {
                            ++lineNumber;

                            if (sourceStream.Position - totalRead > fileSize)
                                break;

                            var line = reader.ReadLineAsMemory();
         
                            if (line.IsEmpty && reader.EndOfStream)
                                break;

                            var success = line.TryParsePriority(out var priority);
                            if (!success)
                                throw new Exception(string.Format(rowBokenFormatMessage, lineNumber, line.Eclipse(100)));

                            queue.Enqueue(null, priority);
                        }

                        // StreamReader undelying stream reads form input file by pages
                        // so we need to finish read input file ending with code like below
                        // because target file size becomes zero for small input files
                        while (!token.IsCancellationRequested)
                        {
                            ++lineNumber;

                            if (reader.EndOfStream || sourceStream.Position != sourceStream.Length)
                                break;

                            var line = reader.ReadLineAsMemory();

                            if (line.IsEmpty && reader.EndOfStream)
                                break;

                            var success = line.TryParsePriority(out var priority);
                            if (!success)
                                throw new Exception(string.Format(rowBokenFormatMessage, lineNumber, line.Eclipse(100)));

                            queue.Enqueue(null, priority);
                        }

                        token.ThrowIfCancellationRequested();
                        totalRead = sourceStream.Position;
                        var fileName = $"{file}{_SortedFileExtension}{_TempFileExtension}";

                        tasks.Add(Task.Run(() => 
                        {
                            using var stream = File.OpenWrite(Path.Combine(_settings.IOPath.SortWritePath, fileName));
                            using var writer = new StreamWriter(stream, encoding, bufferSize: _settings.SortOutputBufferSize);
                            if (!_settings.Format.UsePreamble)
                                writer.SkipPreamble();

                            var builder = new StringBuilder(); (ReadOnlyMemory<char> Str, BigInteger Int) row;
                            while (queue.TryDequeue(out _, out row) && !token.IsCancellationRequested) 
                            {
                                writer.WriteLine(builder.Append(row.Int).Append(separator).Append(row.Str));
                                builder.Clear();
                            }
                            token.ThrowIfCancellationRequested();
                        }, token));

                        if (tasks.Count == _settings.SortPageSize)
                        {
                            ++page;
                            const string pageMessage = "Waiting the {0} page to be sorted";
                            msgScope.WriteLine(string.Format(pageMessage, page));
    
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
