using ExtSort.Models.Settings;

using System.Text.Json;
using System.Text.Json.Nodes;

namespace ExtSort.Services.Evaluator 
{
    internal class EvaluatorService 
    {
        private readonly EvaluatorSettings _settings;
        public EvaluatorService(EvaluatorSettings settings) 
        {
            _settings = settings ?? throw new NullReferenceException(nameof(settings));
        }

        /// <summary>
        ///[PROTOTYPE]
        ///1. Sorting phase for k buffers 
        ///Let average permuatition number for the sort provided be floor(n/2)
        ///Then for m comparators defined the number is floor(nm/2)
        ///Take k buffers which are sorted in parallel 1/k * floor(nm/2)
        ///2.Optimal merge passes number depending on the number of chunks provided
        ///Let the number of merge passes equals C 
        ///Let the number of chunks equals M then C = ~M^(1/h) where h is a height of the B-Tree
        ///3. Optimal number of files to merge depending on a disk characteritics
        ///a) HDD
        ///Let the disk random access speed equals s
        ///Let the disk latency equals l
        ///Then the total speed for x files
        ///b) SSD
        /// </summary>
        public void GenerateSettings() 
        {
            var generator = new GeneratorSettings();
            var format = new FormatSettings();
            var sorterCpu = new SorterCPUSettings();
            var sorterIo = new SorterIOSettings();

            var procCount = Environment.ProcessorCount;
            var pageSize = Math.Min(_settings.FileSizeMb, _settings.RamAvailableMb);
            var fileSizePerPage = pageSize / procCount;
            
            var drive = Path.GetPathRoot(Environment.SystemDirectory);
            var tmp = Path.Combine(drive, "Temp", "Files");
            var obj = new JsonObject()
            {
                [nameof(GeneratorSettings)] = new JsonObject() 
                {
                    [nameof(GeneratorSettings.MaxIntegerNumber)] = generator.MaxIntegerNumber,
                    [nameof(GeneratorSettings.MaxWordLength)] = generator.MaxWordLength,
                    [nameof(GeneratorSettings.MinWordLength)] = generator.MinWordLength,
                    [nameof(GeneratorSettings.OutputBufferSize)] = generator.MinWordLength,
                },
                [nameof(SorterSettings)] = new JsonObject() 
                {
                    [nameof(SorterSettings.NumberOfFiles)] = _settings.NumberOfFiles,
                    [nameof(SorterSettings.SortPageSize)] = procCount,
                    [nameof(SorterSettings.SortOutputBufferSize)] = 4096 * 1024,
                    [nameof(SorterSettings.MergePageSize)] = (int)Math.Sqrt(procCount),
                    [nameof(SorterSettings.MergeChunkSize)] = procCount,
                    [nameof(SorterSettings.MergeOutputBufferSize)] = sorterCpu.MergeOutputBufferSize,
                    [nameof(SorterSettings.IOPath)] = new JsonObject() 
                    {
                        [nameof(SorterSettings.IOPath.SortReadPath)] = tmp,
                        [nameof(SorterSettings.IOPath.SortWritePath)] = tmp,
                        [nameof(SorterSettings.IOPath.MergeStartPath)] = tmp,
                        [nameof(SorterSettings.IOPath.MergeStartTargetPath)] = tmp
                    }
                },
                [nameof(SorterCPUSettings)] = new JsonObject() 
                {
                    [nameof(SorterCPUSettings.BufferCapacityLines)] = sorterCpu.BufferCapacityLines
                },
                [nameof(SorterIOSettings)] = new JsonObject()
                {
                    [nameof(SorterIOSettings.SortThenMergeChunkSize)] = sorterIo.SortThenMergeChunkSize,
                    [nameof(SorterIOSettings.SortThenMergePageSize)] = sorterIo.SortThenMergePageSize
                },
                [nameof(FormatSettings)] = new JsonObject() 
                {
                    [nameof(FormatSettings.EncodingName)] = format.EncodingName,
                    [nameof(FormatSettings.UsePreamble)] = format.UsePreamble,
                    [nameof(FormatSettings.ColumnSeparator)] = format.ColumnSeparator,
                    [nameof(FormatSettings.NewLineDelimiter)] = Environment.NewLine.Last(),
                }
            };

            using var jDoc = JsonDocument.Parse(obj.ToJsonString());
            var prettified = JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true });
            var path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            File.WriteAllText(Path.Combine(path, "appsettings.json"), prettified);
        }
    }
}
