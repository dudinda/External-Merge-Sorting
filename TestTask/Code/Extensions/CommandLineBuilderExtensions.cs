using System.CommandLine.Builder;

namespace TestTask.Code.Extensions
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
                        context.ExitCode = 130;
                        break;
                    case OutOfMemoryException:
                        context.ExitCode = 137;
                        break;
                    case Exception:
                        context.ExitCode = 1;
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
