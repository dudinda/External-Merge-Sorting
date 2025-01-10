using Microsoft.Extensions.Configuration;

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Reflection;

using ExtSort.Code.Extensions;
using ExtSort.Services.Factories;

var config = new ConfigurationBuilder();
var path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
config.SetBasePath(path).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

var rootCommand = new RootCommand("External merge sorting with an input generator");
var factory = new CommandFactory(config.Build());
foreach (var cmd in factory.Commands)
{
    rootCommand.AddCommand(cmd);
}
var builder = new CommandLineBuilder(rootCommand);

return await builder.UseDefaults().BuildExceptionHanlder().Build().InvokeAsync(args);
