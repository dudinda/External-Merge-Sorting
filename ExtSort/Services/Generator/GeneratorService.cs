using System.Text;

using ExtSort.Code.Constants;
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
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var words = GeneratorData.Data;
            await using (var writer = new StreamWriter(fileName, append: false, new UTF8Encoding(false), _settings.OutputBufferSize))
            {
                writer.BaseStream.SetLength(sizeB);
                var builder = new StringBuilder();
                var target = new StringBuilder();
                var maxNumber = _settings.MaxIntegerNumber + 1;
                var maxWordLength = _settings.MaxWordLength + 1;
                var minWordLength = _settings.MinWordLength;
                var wordsLength = words.Length;
                while(writer.BaseStream.CanWrite && writer.BaseStream.Position <= sizeB && !token.IsCancellationRequested)
                {
                    var numberOfWords = rnd.Next(minWordLength, maxWordLength);
                    while(numberOfWords-- > 0)
                    {
                        builder.Append($" {words[rnd.Next(wordsLength)]}");
                    }

                    target.Append(rnd.Next(maxNumber)).Append('.').Append(builder);
                    writer.WriteLine(target);
                    builder.Clear();
                    target.Clear();
                }

                token.ThrowIfCancellationRequested();
            }
        }
    }
}
