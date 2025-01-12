using System.Text;

namespace ExtSort.Models.Settings 
{
    public record FormatSettings 
    {
        public string EncodingName { get; init; } = "utf-8";
        public bool UsePreamble { get; init; } = true;
        public string ColumnSeparator { get; init; } = ".";
        public char NewLineDelimiter { get; init; } = '\n';

        public bool Validate(out StringBuilder errors) 
        {
            errors = new StringBuilder();
            if (string.IsNullOrEmpty(EncodingName))
                errors.AppendLine("The encoding name is not specified");
            if (string.IsNullOrEmpty(ColumnSeparator))
                errors.AppendLine("The column separator string is not specified");
            if (!Encoding.GetEncodings().Any(arg => arg.Name == EncodingName))
                errors.AppendLine($"The provided encoding {EncodingName} does not exist");

            return errors.Length == 0;
        }
    }
}
