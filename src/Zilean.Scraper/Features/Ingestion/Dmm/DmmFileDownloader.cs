namespace Zilean.Scraper.Features.Ingestion.Dmm;

public class DmmFileDownloader(ILogger<DmmFileDownloader> logger, ZileanConfiguration configuration)
{
    private const string RepoUrl = "https://github.com/debridmediamanager/hashlists.git";
    private const string RepoBranch = "main";
    private const int MaxRetryAttempts = 5;
    private static readonly TimeSpan _initialRetryDelay = TimeSpan.FromSeconds(5);

    private static readonly IReadOnlyCollection<string> _filesToIgnore =
    [
        "index.html",
        "404.html",
        "dedupe.sh",
        "CNAME",
        ".git",
    ];

    public async Task<string> DownloadFileToTempPath(DmmLastImport? dmmLastImport, CancellationToken cancellationToken)
    {
        logger.LogInformation("Syncing DMM Hashlists");

        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data", "DMMHashlists");

        if (dmmLastImport is not null)
        {
            if (DateTime.UtcNow - dmmLastImport.OccuredAt < TimeSpan.FromMinutes(configuration.Dmm.MinimumReDownloadIntervalMinutes))
            {
                logger.LogInformation("DMM Hashlists sync not required as last sync was less than the configured {Minutes} minutes re-download interval set in DMM Configuration.", configuration.Dmm.MinimumReDownloadIntervalMinutes);
                return dataDirectory;
            }
        }

        var repoDirectory = Path.Combine(dataDirectory, "repo");
        var gitDirectory = Path.Combine(repoDirectory, ".git");

        var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        var repoUrlWithAuth = GetRepoUrlWithAuth(githubToken);

        if (Directory.Exists(gitDirectory))
        {
            logger.LogInformation("Repository exists, pulling latest changes");
            await GitPullAsync(repoDirectory, repoUrlWithAuth, cancellationToken);
        }
        else
        {
            logger.LogInformation("Repository does not exist, cloning");
            EnsureDirectoryIsClean(dataDirectory);
            await GitCloneAsync(repoUrlWithAuth, repoDirectory, cancellationToken);
        }

        CopyFilesToDataDirectory(repoDirectory, dataDirectory);

        logger.LogInformation("Synced Repository to {DataDirectory}", dataDirectory);

        return dataDirectory;
    }

    private string GetRepoUrlWithAuth(string? githubToken)
    {
        if (string.IsNullOrWhiteSpace(githubToken))
        {
            logger.LogDebug("No GITHUB_TOKEN environment variable found. Git operations may be rate limited");
            return RepoUrl;
        }

        logger.LogInformation("Using GITHUB_TOKEN for authenticated Git operations to avoid rate limiting");
        // Format: https://<token>@github.com/owner/repo.git
        return RepoUrl.Replace("https://", $"https://{githubToken}@");
    }

    private async Task GitCloneAsync(string repoUrl, string targetDirectory, CancellationToken cancellationToken)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"clone --depth 1 --branch {RepoBranch} --single-branch \"{repoUrl}\" \"{targetDirectory}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            await RunGitProcessAsync(process, "clone", cancellationToken);
        }, "clone", targetDirectory, cancellationToken);
    }

    private async Task GitPullAsync(string repoDirectory, string repoUrl, CancellationToken cancellationToken)
    {
        // Update the remote URL in case the token changed
        var setUrlProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"-C \"{repoDirectory}\" remote set-url origin \"{repoUrl}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        await RunGitProcessAsync(setUrlProcess, "remote set-url", cancellationToken);

        // Pull latest changes with retry
        await ExecuteWithRetryAsync(async () =>
        {
            var pullProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"-C \"{repoDirectory}\" pull --ff-only",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            await RunGitProcessAsync(pullProcess, "pull", cancellationToken);
        }, "pull", repoDirectory, cancellationToken);
    }

    private async Task RunGitProcessAsync(Process process, string operation, CancellationToken cancellationToken)
    {
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            logger.LogError("Git {Operation} failed with exit code {ExitCode}: {Error}", operation, process.ExitCode, error);
            throw new InvalidOperationException($"Git {operation} failed: {error}");
        }

        if (!string.IsNullOrWhiteSpace(output))
        {
            logger.LogDebug("Git {Operation} output: {Output}", operation, output);
        }
    }

    private async Task ExecuteWithRetryAsync(Func<Task> operation, string operationName, string targetDirectory, CancellationToken cancellationToken)
    {
        var attempt = 0;
        var delay = _initialRetryDelay;

        while (true)
        {
            attempt++;
            try
            {
                await operation();
                return;
            }
            catch (InvalidOperationException ex) when (attempt < MaxRetryAttempts && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    "Git {Operation} attempt {Attempt}/{MaxAttempts} failed. Retrying in {Delay} seconds... Error: {Error}",
                    operationName,
                    attempt,
                    MaxRetryAttempts,
                    delay.TotalSeconds,
                    ex.Message);

                // Clean up the target directory before retry for clone operations
                if (operationName == "clone" && Directory.Exists(targetDirectory))
                {
                    try
                    {
                        Directory.Delete(targetDirectory, true);
                    }
                    catch (Exception cleanupEx)
                    {
                        logger.LogWarning("Failed to clean up directory {Directory} before retry: {Error}", targetDirectory, cleanupEx.Message);
                    }
                }

                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60)); // Exponential backoff, max 60 seconds
            }
        }
    }

    private void CopyFilesToDataDirectory(string repoDirectory, string dataDirectory)
    {
        var files = Directory.GetFiles(repoDirectory);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            if (_filesToIgnore.Contains(fileName))
            {
                continue;
            }

            var destPath = Path.Combine(dataDirectory, fileName);
            File.Copy(file, destPath, true);
        }
    }

    private static void EnsureDirectoryIsClean(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
        }

        Directory.CreateDirectory(directory);
    }
}
