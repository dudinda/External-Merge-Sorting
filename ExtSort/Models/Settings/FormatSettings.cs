using System.Text;

namespace ExtSort.Models.Settings 
{
    public record FormatSettings 
    {
        public string EncodingName { get; init; } = "utf-8";
        public bool UsePreamble { get; init; } = true;
        public string ColumnSeparator { get; init; } = ".";
        public char? NewLineDelimiter { get; init; } = '\n';
        public int MaxNumberOfDigitsBigInt { get; init; } = 6;
      
        public bool Validate(out StringBuilder errors) 
        {
            errors = new StringBuilder();
            if (string.IsNullOrEmpty(EncodingName))
                errors.AppendLine("The encoding name is not specified");
            if (string.IsNullOrEmpty(ColumnSeparator))
                errors.AppendLine("The column separator string is not specified");
            if (NewLineDelimiter == null)
                errors.AppendLine("The new line delimiter character is not specified");
            if (NewLineDelimiter == char.MinValue)
                errors.AppendLine("The new line delimiter must differs from the NULL symbol");
            if (!Encoding.GetEncodings().Any(arg => arg.Name == EncodingName))
                errors.AppendLine($"The provided encoding {EncodingName} does not exist");
            if (MaxNumberOfDigitsBigInt < 1)
                errors.AppendLine("Integers cannot have less than 1 digit");

            return errors.Length == 0;
        }
    }
}
