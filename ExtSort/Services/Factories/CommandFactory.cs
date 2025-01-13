using ExtSort.Code.Constants;
using ExtSort.Models.Binders;
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
                var binder = new GeneratorBinder(ctx.BindingContext.ParseResult);
                var settings = _config.GetSection(nameof(GeneratorSettings)).Get<GeneratorSettings>();
                _config.GetSection(nameof(FormatSettings)).Bind(settings.Format);
                if (!settings.Validate(out var errors))
                    throw new InvalidOperationException(errors.ToString());

                var service = new GeneratorService(settings);
                using var timer = new SimpleTimer("Generating a file");
                await service.Generate(binder.TargetFileName, binder.TargetFileSizeKb, ctx.GetCancellationToken());
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
                var binder = new SorterBinder(ctx.BindingContext.ParseResult);
                var factory = new SortModeFactory(_config);
                using var service = factory.Get(binder.Mode);
                using var timer = new SimpleTimer("Sorting a file");
                await service.SortFile(binder.SourceFileName, binder.TargetFileName, ctx.GetCancellationToken());
            });

            return cmd;
        }

        private Command BuildEvaluateCommand() 
        {
            var cmd = new Command(Verbs.EvaluateVerb, VerbDescriptions.EvaluateDesc);
            cmd.AddAlias(Verbs.EvaluateVerb[0..1]);
            cmd.AddAlias(Verbs.EvaluateVerb[0..4]);
            foreach (var arg in ArgumentFactory.EvaluatorArguments.Value.Values)
                cmd.AddArgument(arg);

            cmd.SetHandler((ctx) => 
            {
                var binder = new EvaluatorBinder(ctx.BindingContext.ParseResult);
                var service = new SettingsService();
                service.GenerateSettings(binder.FileSizeMb, binder.RamAvailableMb);
            });

            return cmd;
        }
    }
}
