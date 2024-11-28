using ExtSort.Code.Enums;

using System.CommandLine.Builder;

namespace ExtSort.Code.Extensions
{
    public static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder BuildExceptionHanlder(this CommandLineBuilder builder)
        {
            builder.UseExceptionHandler((ex, context) =>
            {
                switch (ex)
                {
                    case OperationCanceledException:
                        context.ExitCode = (int)ExitCode.OperationCancelled;
                        break;
                    case OutOfMemoryException:
                        context.ExitCode = (int)ExitCode.OutOfMemory;
                        break;
                    case Exception:
                        context.ExitCode = (int)ExitCode.Custom;
                        break;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.Write(context.LocalizationResources.ExceptionHandlerHeader());
                Console.Error.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            });

            return builder;
        }
    }
}
