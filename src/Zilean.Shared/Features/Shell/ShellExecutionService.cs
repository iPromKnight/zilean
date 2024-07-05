namespace Zilean.Shared.Features.Shell;

public interface IShellExecutionService
{
    Task ExecuteCommand(ShellCommandOptions options);
}

public class ShellExecutionService(ILogger<ShellExecutionService> logger) : IShellExecutionService
{
    public async Task ExecuteCommand(ShellCommandOptions options)
    {
        try
        {
            var arguments = options.ArgumentsBuilder.RenderArguments(propertyKeySeparator: options.PropertyKeySeparator);

            if (options.ShowOutput)
            {
                logger.LogInformation(string.IsNullOrEmpty(options.PreCommandMessage)
                    ? $"Executing: {options.Command} {arguments}"
                    : options.PreCommandMessage);
            }

            var executionDirectory = string.IsNullOrEmpty(options.WorkingDirectory)
                ? Directory.GetCurrentDirectory()
                : options.WorkingDirectory;

            await using var stdOut = Console.OpenStandardOutput();
            await using var stdErr = Console.OpenStandardError();

            await Cli.Wrap(options.Command)
                .WithWorkingDirectory(executionDirectory)
                .WithArguments(arguments)
                .WithEnvironmentVariables(options.EnvironmentVariables)
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStream(stdOut))
                .WithStandardErrorPipe(PipeTarget.ToStream(stdErr))
                .ExecuteAsync(options.CancellationToken);
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Command execution was cancelled");
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Command execution was cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing command");
        }
    }
}
