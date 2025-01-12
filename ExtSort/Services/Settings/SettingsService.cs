using ExtSort.Models.Settings;

using System.Text.Json;
using System.Text.Json.Nodes;

namespace ExtSort.Services.Settings 
{
    internal class SettingsService 
    {
        public void GenerateSettings() 
        {
            var generator = new GeneratorSettings();
            var format = new FormatSettings();
            var sorterCpu = new SorterCPUSettings();
            var sorterIo = new SorterIOSettings();
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
                    [nameof(SorterSettings.NumberOfFiles)] = sorterCpu.NumberOfFiles,
                    [nameof(SorterSettings.SortPageSize)] = Environment.ProcessorCount,
                    [nameof(SorterSettings.SortOutputBufferSize)] = 4096 * 1024,
                    [nameof(SorterSettings.MergePageSize)] = (int)Math.Sqrt(Environment.ProcessorCount),
                    [nameof(SorterSettings.MergeChunkSize)] = Environment.ProcessorCount,
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
