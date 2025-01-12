using System.Text;

using ExtSort.Code.Constants;
using ExtSort.Code.Extensions;
using ExtSort.Models.Settings;

namespace ExtSort.Services.Generator
{
    public class GeneratorService
    {
        private readonly GeneratorSettings _settings;

        public GeneratorService(GeneratorSettings settings)
        {
            _settings = settings;
        }

        public async Task Generate(string fileName, long sizeKb, CancellationToken token)
        {
            var sizeB = sizeKb * 1024;
            Console.WriteLine("Generating..");

            var encoding = Encoding.GetEncoding(_settings.Format.EncodingName);
            await using (var writer = new StreamWriter(fileName, append: false, encoding, _settings.OutputBufferSize))
            {
                writer.BaseStream.SetLength(sizeB);
                if (!_settings.Format.UsePreamble && writer.BaseStream.Position > 0)
                    writer.SkipPreamble();

                var words = GeneratorData.Data;
                var maxNumber = _settings.MaxIntegerNumber + 1;
                var maxWordLength = _settings.MaxWordLength + 1;
                var minWordLength = _settings.MinWordLength;
                var wordsLength = words.Length;
                var separator = _settings.Format.ColumnSeparator;

                var builder = new StringBuilder();
                var target = new StringBuilder();

                var rnd = new Random(Guid.NewGuid().GetHashCode());
                while (writer.BaseStream.CanWrite && writer.BaseStream.Position <= sizeB && !token.IsCancellationRequested)
                {
                    var numberOfWords = rnd.Next(minWordLength, maxWordLength);
                    while(numberOfWords-- > 0)
                        builder.Append($" {words[rnd.Next(wordsLength)]}");

                    target.Append(rnd.Next(maxNumber)).Append(separator).Append(builder);
                    writer.WriteLine(target);
                    builder.Clear();
                    target.Clear();
                }
                token.ThrowIfCancellationRequested();
            }
        }
    }
}
