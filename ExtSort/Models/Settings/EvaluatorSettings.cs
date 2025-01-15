using ExtSort.Models.Binders;

using System.Text;

namespace ExtSort.Models.Settings 
{
    internal record EvaluatorSettings(EvaluatorBinder binder) : EvaluatorBinder 
    {
        public bool Validate(out StringBuilder errors) 
        {
            errors = new StringBuilder();
            if (NumberOfFiles < 1)
                errors.AppendLine("The number of files must be equal or greater than 1");
            if (RamAvailableMb <= 0)
                errors.AppendLine("The RAM available must be greater than 0");
            if (FileSizeMb <= 0)
                errors.AppendLine("The file size must be greater than 0");
            if (DiskLatencyMs < 0)
                errors.AppendLine("The disk latency cannot be negative");
            if (DiskRandomReadSpeedMbs <= 0)
                errors.AppendLine("The disk random access speed must be greather than 0");

            return errors.Length == 0;
        }
    }
}
