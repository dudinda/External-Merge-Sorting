﻿using System.Text;

namespace ExtSort.Models.Settings
{
    public record ReadWritePath
    {
        public string SplitReadPath { get; init; } = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        public string SortReadPath { get; init; }
        public string SortWritePath { get; init; }
        public string MergeStartPath { get; init; }
        public string MergeStartTargetPath { get; init; }

        public bool Validate(out StringBuilder errors)
        {
            errors = new StringBuilder();
            if (string.IsNullOrEmpty(SplitReadPath))
                errors.AppendLine("The target path to split chunks is not specified.");
            if (string.IsNullOrEmpty(SortReadPath))
                errors.AppendLine("The target path to get unsorted files is not specified.");
            if (string.IsNullOrEmpty(SortWritePath))
                errors.AppendLine("The target path to sort unsorted files is not specified.");
            if (string.IsNullOrEmpty(MergeStartPath))
                errors.AppendLine("The target path to select sorted files for merge is not specified.");
            if (string.IsNullOrEmpty(MergeStartTargetPath))
                errors.AppendLine("The target path to merge files is not specified.");

            Directory.CreateDirectory(SortReadPath);
            Directory.CreateDirectory(SortWritePath);
            Directory.CreateDirectory(MergeStartPath);
            Directory.CreateDirectory(MergeStartTargetPath);

            return errors.Length == 0;
        }
    }
}
