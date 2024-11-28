using ExtSort.Code.Enums;

namespace ExtSort.Models.Arguments
{
    internal class SorterArgument
    {
        public string TargetFileName { get; set; }
        public string SourceFileName { get; set; }
        public SortMode Mode { get; set; }
    }
}
