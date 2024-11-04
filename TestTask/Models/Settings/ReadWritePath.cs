using System.Reflection;

namespace TestTask.Models.Settings
{
    public class ReadWritePath
    {
        public string SplitReadPath { get; init; } = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public string SortReadPath { get; init; }
        public string SortWritePath { get; init; }
        public string MergeStartTargetPath { get; init; }
    }
}
