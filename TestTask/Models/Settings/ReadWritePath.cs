﻿using System.Reflection;
using System.Text;

namespace TestTask.Models.Settings
{
    public class ReadWritePath
    {
        public string SplitReadPath { get; init; } = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public string SortReadPath { get; init; }
        public string SortWritePath { get; init; }
        public string MergeStartTargetPath { get; init; }

        public bool Validate(out StringBuilder errors)
        {
            errors = new StringBuilder();
            if (string.IsNullOrEmpty(SplitReadPath))
                errors.AppendLine("The target path for split chuck is not specified.");
            if (string.IsNullOrEmpty(SortReadPath))
                errors.AppendLine("The target path to get unsorted files is not specified.");
            if (string.IsNullOrEmpty(SortWritePath))
                errors.AppendLine("The target path to sort unsorted files is not specified.");
            if (string.IsNullOrEmpty(MergeStartTargetPath))
                errors.AppendLine("The target path to select sorted files for merge is not specified.");

            return errors.Length == 0;
        }
    }
}
