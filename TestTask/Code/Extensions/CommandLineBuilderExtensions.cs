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
                    case var _ when ex is OperationCanceledException:
                        context.ExitCode = -1;
                        break;
                    case var _ when ex is Exception:
                        context.ExitCode = -2;
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
