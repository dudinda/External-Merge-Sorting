using System.Text;

namespace ExtSort.Models.Settings
{
    public class SorterCPUSettings : SorterSettings
    {
        public int BufferCapacityLines { get; init; } = 720000;

        public override bool Validate(out StringBuilder errors)
        {
            errors = new StringBuilder();
            if (BufferCapacityLines <= 0)
                errors.AppendLine("Buffer capacity must be non-negative");
            if (!base.Validate(out var ioErrors))
                errors.Append(ioErrors);

            return errors.Length == 0;
        }
    }
}
