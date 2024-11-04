using System.Text;

using TestTask.Code.Constants;
using TestTask.Models.Settings;

namespace TestTask.Services.Generator
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
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fileStream.SetLength(sizeB);
            }
            Console.WriteLine("Generating..");
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var words = GeneratorData.Data;
            await using (var writer = new StreamWriter(fileName, append: false, Encoding.UTF8, 65536))
            {
                var builder = new StringBuilder();
                while(writer.BaseStream.CanWrite && writer.BaseStream.Position <= sizeB && !token.IsCancellationRequested)
                {
                    var numberOfWords = rnd.Next(1, _settings.MaxWordLength);
                    while(numberOfWords-- > 0)
                    {
                        builder.Append($" {words[rnd.Next(words.Length - 1)]}");
                    }
                    var result = $"{rnd.Next(_settings.MaxIntegerNumber)}.{builder.ToString()}";
                    writer.WriteLine(result);
                    builder.Clear();
                }

                token.ThrowIfCancellationRequested();
            }
        }
    }
}
