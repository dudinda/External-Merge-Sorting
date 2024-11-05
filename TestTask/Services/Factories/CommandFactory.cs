using Microsoft.Extensions.Configuration;

using System.CommandLine;

using TestTask.Code.Constants;
using TestTask.Models.Arguments;
using TestTask.Models.Settings;
using TestTask.Models.Timer;
using TestTask.Services.Generator;
using TestTask.Services.Sorter;

namespace TestTask.Services.Factories
{
    internal class CommandFactory
    {
        private readonly IConfiguration _config;

        public CommandFactory(IConfiguration config)
        {
            _config = config;
        }

        public IEnumerable<Command> Commands
        {
            get
            {
                yield return BuildGenerateCommand();
                yield return BuildSortCommand();
            }
        }

        private Command BuildGenerateCommand()
        {
            var cmd = new Command(Verbs.GeneratorVerb, VerbDescriptions.GeneratorDesc);
            cmd.AddAlias(Verbs.GeneratorVerb[0..1]);
            foreach (var arg in ArgumentFactory.GeneratorArguments.Value.Values)
            {
                cmd.AddArgument(arg);
            }

            cmd.SetHandler(async (ctx) =>
            {
                var args = ArgumentFactory.GeneratorArguments.Value;
                var parser = ctx.BindingContext.ParseResult;
                var result = new GeneratorArgument();
                result.TargetFileName = (string)parser.GetValueForArgument(args[nameof(result.TargetFileName)]);
                var size = parser.GetValueForArgument(args[nameof(result.TargetFileSizeKb)])?.ToString();
                if (!long.TryParse(size, out var fileSize))
                    throw new InvalidCastException("The length of the output file is in incorrect format.");
                result.TargetFileSizeKb = fileSize;

                var settings = _config.GetSection(nameof(GeneratorSetting)).Get<GeneratorSetting>();
                if (!settings.Validate(out var errors))
                    throw new InvalidOperationException(errors.ToString());

                var service = new GeneratorService(settings);
                using var timer = new SimpleTimer("Generating a file");
                await service.Generate(result.TargetFileName, result.TargetFileSizeKb, ctx.GetCancellationToken());
            });

            return cmd;
        }

        private Command BuildSortCommand()
        {
            var cmd = new Command(Verbs.SortVerb, VerbDescriptions.SorterDesc);
            cmd.AddAlias(Verbs.SortVerb[0..1]);
            foreach (var arg in ArgumentFactory.SorterArguments.Value.Values)
            {
                cmd.AddArgument(arg);
            }

            cmd.SetHandler(async (ctx) =>
            {
                var args = ArgumentFactory.SorterArguments.Value;
                var parser = ctx.BindingContext.ParseResult;
                var result = new SorterArgument();
                result.TargetFileName = (string)parser.GetValueForArgument(args[nameof(result.TargetFileName)]);
                result.SourceFileName = (string)parser.GetValueForArgument(args[nameof(result.SourceFileName)]);

                var settings = _config.GetSection(nameof(SorterSetting)).Get<SorterSetting>();
                if (!settings.Validate(out var errors))
                    throw new InvalidOperationException(errors.ToString());

                var service = new SorterService(settings);
                using var timer = new SimpleTimer("Sorting a file");
                await service.SortFile(result.SourceFileName, result.TargetFileName, ctx.GetCancellationToken());
            });

            return cmd;
        }
    }
}
