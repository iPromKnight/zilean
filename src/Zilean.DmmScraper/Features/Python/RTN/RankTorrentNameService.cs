namespace Zilean.DmmScraper.Features.Python.RTN;

public class RankTorrentNameService
{
    private const string RtnModuleName = "RTN";
    private readonly Task _initAsync;
    private IntPtr _mainThreadState;
    private bool _isInitialized;
    private dynamic? _sys;
    private dynamic? _rtn;
    private readonly ILogger<RankTorrentNameService> _logger;
    private int _currentBatchNumber;
    private int _totalBatches;

    public RankTorrentNameService(ILogger<RankTorrentNameService> logger)
    {
        _logger = logger;
        _initAsync = InitializePythonEngine();
    }

    public Task StopPythonEngine()
    {
        PythonEngine.EndAllowThreads(_mainThreadState);
        PythonEngine.Shutdown();

        return Task.CompletedTask;
    }

    public async Task ParseAndPopulateAsync(List<ExtractedDmmEntry> torrents, int batchSize = 1000, bool trashGarbage = false, bool logErrors = false, bool throwOnErrors = false, int maxConcurrentTasks = 5)
    {
        await _initAsync;

        _currentBatchNumber = 0;
        _logger.LogInformation("RTN: Parsing {Count} torrents", torrents.Count);

        var semaphore = new SemaphoreSlim(maxConcurrentTasks);
        var batchedTorrents = BatchTorrents(torrents, batchSize).ToList();
        _totalBatches = batchedTorrents.Count;

        _logger.LogInformation("RTN: Processing {Count} batches of size {Size} concurrently with {Workers} workers", _totalBatches, batchSize, maxConcurrentTasks);

        var runningTasks = new List<Task>();
        foreach (var batch in batchedTorrents)
        {
            if (runningTasks.Count >= maxConcurrentTasks)
            {
                var completedTask = await Task.WhenAny(runningTasks);
                runningTasks.Remove(completedTask);
            }

            var task = ProcessBatchWithSemaphoreAsync(batch, semaphore, trashGarbage, logErrors, throwOnErrors);
            runningTasks.Add(task);
        }

        await Task.WhenAll(runningTasks); // Ensure all tasks are completed
    }

    private async Task ProcessBatchWithSemaphoreAsync(List<ExtractedDmmEntry> batch, SemaphoreSlim semaphore, bool trashGarbage, bool logErrors, bool throwOnErrors)
    {
        await semaphore.WaitAsync();

        try
        {
            await ProcessBatchAsync(batch, trashGarbage, logErrors, throwOnErrors);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task ProcessBatchAsync(List<ExtractedDmmEntry> batch, bool trashGarbage, bool logErrors, bool throwOnErrors)
    {
        await _initAsync;

        Interlocked.Increment(ref _currentBatchNumber);

        var torrentsToParse = batch.ToDictionary(x => x.InfoHash!, x => x.Filename);

        foreach (var entry in torrentsToParse)
        {
            var result = Parse(entry.Value, trashGarbage, logErrors, throwOnErrors);

            if (result.Success)
            {
                batch.Single(x => x.InfoHash == entry.Key).RtnResponse = result.Response;
            }
        }

        _logger.LogInformation("RTN: Processed batch {CurrentBatch}/{TotalBatches}", _currentBatchNumber, _totalBatches);
    }

    private ParseTorrentTitleResponse Parse(string title, bool trashGarbage = false, bool logErrors = false, bool throwOnErrors = false)
    {
        try
        {
            using var gil = Py.GIL();
            var result = _rtn?.parse(title, trashGarbage);
            return ParseResult(result);
        }
        catch (Exception ex)
        {
            if (logErrors)
            {
                _logger.LogError(ex, "Python Error: {Message} ({OperationName})", ex.Message, nameof(Parse));
            }

            if (throwOnErrors)
            {
                throw;
            }

            return new(false, null);
        }
    }

    private Task InitializePythonEngine()
    {
        if (_isInitialized)
        {
            return Task.CompletedTask;
        }

        try
        {
            var pythonDllEnv = Environment.GetEnvironmentVariable("PYTHONNET_PYDLL");

            if (string.IsNullOrWhiteSpace(pythonDllEnv))
            {
                _logger.LogWarning("PYTHONNET_PYDLL env is not set. Exiting Application");
                Environment.Exit(1);
                return Task.CompletedTask;
            }

            Runtime.PythonDLL = pythonDllEnv;
            PythonEngine.Initialize();
            _mainThreadState = PythonEngine.BeginAllowThreads();
            using var gil = Py.GIL();
            _sys = Py.Import("sys");
            _sys.path.append(Path.Combine(AppContext.BaseDirectory, "python"));
            _rtn = Py.Import(RtnModuleName);
            _isInitialized = true;
            _logger.LogInformation("Python engine initialized");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to initialize Python engine: {e.Message}");
            Environment.Exit(1);
        }

        return Task.CompletedTask;
    }

    private static ParseTorrentTitleResponse ParseResult(dynamic result)
    {
        try
        {
            if (result == null)
            {
                return new ParseTorrentTitleResponse(false, null);
            }

            var json = result.model_dump_json()?.As<string?>();

            if (json is null || string.IsNullOrEmpty(json))
            {
                return new ParseTorrentTitleResponse(false, null);
            }

            var mediaType = result.GetAttr("type")?.As<string>();

            if (string.IsNullOrEmpty(mediaType))
            {
                return new ParseTorrentTitleResponse(false, null);
            }

            var response = JsonSerializer.Deserialize<RtnResponse>(json);

            response.IsMovie = mediaType.Equals("movie", StringComparison.OrdinalIgnoreCase);

            return new(true, response);
        }
        catch
        {
            return new ParseTorrentTitleResponse(false, null);
        }
    }

    private static IEnumerable<List<ExtractedDmmEntry>> BatchTorrents(List<ExtractedDmmEntry> torrents, int batchSize)
    {
        for (int i = 0; i < torrents.Count; i += batchSize)
        {
            yield return torrents.GetRange(i, Math.Min(batchSize, torrents.Count - i));
        }
    }
}
