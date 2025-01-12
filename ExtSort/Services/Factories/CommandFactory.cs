using ExtSort.Code.Constants;
using ExtSort.Code.Enums;
using ExtSort.Models.Arguments;
using ExtSort.Models.Settings;
using ExtSort.Models.Timer;
using ExtSort.Services.Generator;
using ExtSort.Services.Settings;

using Microsoft.Extensions.Configuration;

using System.CommandLine;

namespace ExtSort.Services.Factories 
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
                yield return BuildEvaluateCommand();
            }
        }

        private Command BuildGenerateCommand()
        {
            var cmd = new Command(Verbs.GeneratorVerb, VerbDescriptions.GeneratorDesc);
            cmd.AddAlias(Verbs.GeneratorVerb[0..1]);
            foreach (var arg in ArgumentFactory.GeneratorArguments.Value.Values)
                cmd.AddArgument(arg);

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

                var settings = _config.GetSection(nameof(GeneratorSettings)).Get<GeneratorSettings>();
                _config.GetSection(nameof(FormatSettings)).Bind(settings.Format);
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
                cmd.AddArgument(arg);

            cmd.SetHandler(async (ctx) =>
            {
                var args = ArgumentFactory.SorterArguments.Value;
                var parser = ctx.BindingContext.ParseResult;
                var result = new SorterArgument();
                result.TargetFileName = (string)parser.GetValueForArgument(args[nameof(result.TargetFileName)]);
                result.SourceFileName = (string)parser.GetValueForArgument(args[nameof(result.SourceFileName)]);
                result.Mode = (SortMode)parser.GetValueForArgument(args[nameof(result.Mode)]);

                var factory = new SortModeFactory(_config);
                using var service = factory.Get(result.Mode);
                using var timer = new SimpleTimer("Sorting a file");
                await service.SortFile(result.SourceFileName, result.TargetFileName, ctx.GetCancellationToken());
            });

            return cmd;
        }

        private Command BuildEvaluateCommand() 
        {
            var cmd = new Command(Verbs.EvaluateVerb, VerbDescriptions.EvaluateDesc);
            cmd.AddAlias(Verbs.EvaluateVerb[0..1]);
            cmd.AddAlias(Verbs.EvaluateVerb[0..4]);

            cmd.SetHandler(async (ctx) => 
            {
                var service = new SettingsService();
                service.GenerateSettings();
            });

            return cmd;
        }
    }
}
