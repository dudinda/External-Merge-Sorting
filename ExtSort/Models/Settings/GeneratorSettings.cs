﻿using System.Text;

namespace ExtSort.Models.Settings
{
    public record GeneratorSettings 
    {
        public int MaxIntegerNumber { get; init; } = 15000;
        public int MaxWordLength { get; init; } = 4;
        public int MinWordLength { get; init; } = 1;
        public int OutputBufferSize { get; init; } = 4194304;
        public FormatSettings Format {get; init; } = new FormatSettings();

        public bool Validate(out StringBuilder errors)
        {
            errors = new StringBuilder();
            if (MaxWordLength < 1)
                errors.AppendLine("The number of maximum generated words must be greater than 0");
            if (MinWordLength < 1)
                errors.AppendLine("The number of minimum generated words must be greater than 0");
            if (MinWordLength > MaxWordLength)
                errors.AppendLine("The number of minimum generated words must be lesser than maximum");
            if (!Format.Validate(out var formatErrors))
                errors.Append(formatErrors);

            return errors.Length == 0;
        }
    }
}
