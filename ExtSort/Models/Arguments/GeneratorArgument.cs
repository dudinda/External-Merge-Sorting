namespace ExtSort.Models.Arguments 
{
    internal record GeneratorArgument
    {
        public string TargetFileName { get; set; }
        public long TargetFileSizeKb { get; set; }
    }
}
