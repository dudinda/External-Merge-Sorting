using System.Text;

namespace TestTask.Models.Settings
{
    public class GeneratorSetting
    {
        public int MaxIntegerNumber { get; init; } = 15000;
        public int MaxWordLength { get; init; } = 4;

        public bool Validate(out StringBuilder errors)
        {
            errors = new StringBuilder();
            if (MaxWordLength <= 0)
                errors.AppendLine("The number of generated words must be greater than 0");

            return errors.Length == 0;
        }
    }
}
