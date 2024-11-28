﻿using System.Text;

using ExtSort.Code.Constants;
using ExtSort.Models.Settings;

namespace ExtSort.Services.Generator
{
    public class GeneratorService
    {
        private readonly GeneratorSetting _settings;

        public GeneratorService(GeneratorSetting settings)
        {
            _settings = settings;
        }

        public async Task Generate(string fileName, long sizeKb, CancellationToken token)
        {
            var sizeB = sizeKb * 1024;
            Console.WriteLine("Generating..");
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var words = GeneratorData.Data;
            await using (var writer = new StreamWriter(fileName, append: false, Encoding.UTF8, 65536))
            {
                writer.BaseStream.SetLength(sizeB);
                var builder = new StringBuilder();
                var maxNumber = _settings.MaxIntegerNumber + 1;
                var maxWordLength = _settings.MaxWordLength + 1;
                var wordsLength = words.Length;
                while(writer.BaseStream.CanWrite && writer.BaseStream.Position <= sizeB && !token.IsCancellationRequested)
                {
                    var numberOfWords = rnd.Next(1, maxWordLength);
                    while(numberOfWords-- > 0)
                    {
                        builder.Append($" {words[rnd.Next(wordsLength)]}");
                    }
                    var result = $"{rnd.Next(maxNumber)}.{builder.ToString()}";
                    writer.WriteLine(result);
                    builder.Clear();
                }

                token.ThrowIfCancellationRequested();
            }
        }
    }
}
