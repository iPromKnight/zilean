namespace Zilean.Shared.Features.Shell;

[ExcludeFromCodeCoverage]
public sealed class ShellCommandOptions
{
    public string? Command { get; set; }
    public ArgumentsBuilder? ArgumentsBuilder { get; set; } = new();
    public bool NonInteractive { get; set; }
    public bool ShowOutput { get; set; } = false;
    public string? WorkingDirectory { get; set; }
    public char PropertyKeySeparator { get; set; } = ' ';
    public string? PreCommandMessage { get; set; }
    public string? SuccessCommandMessage { get; set; }
    public string? FailureCommandMessage { get; set; }
    public Dictionary<string, string?> EnvironmentVariables { get; set; } = [];
    public CancellationToken CancellationToken { get; set; }
}
