using Microsoft.Extensions.Configuration;

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Reflection;

using TestTask.Code.Extensions;
using TestTask.Services.Factories;

var config = new ConfigurationBuilder();
var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
config.SetBasePath(path).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var rootCommand = new RootCommand("Altium Test Task by Dudin D.A., 2024, Nov.");
var factory = new CommandFactory(config.Build());
foreach (var cmd in factory.Commands)
{
    rootCommand.AddCommand(cmd);
}
var builder = new CommandLineBuilder(rootCommand);

return await builder.UseDefaults().BuildExceptionHanlder().Build().InvokeAsync(args);
