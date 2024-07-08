namespace Zilean.DmmScraper.Features.Python;

public class ParseTorrentNameService
{
    private readonly Task _initAsync;
    private IntPtr _mainThreadState;
    private bool _isInitialized;
    private dynamic? _sys;
    private dynamic? _runProcessBatches;
    private readonly ILogger<ParseTorrentNameService> _logger;

    private const string CreatePythonParserScript =
        """
        from PTT.handlers import add_defaults
        from PTT.parse import Parser
        import asyncio
        from loguru import logger

        logger.remove()
        logger.add(lambda msg: print(msg, end=''), format="[{time:HH:mm:ss}] | {level} | \"<green>Zilean.DmmScraper.Features.Python.ParseTorrentNameService</green>\" | {message}", colorize=True)

        sem = None

        async def parse_torrent(title):
            async with sem:
                try:
                    result = ptt.parse(title)
                    return result
                except Exception as e:
                    logger.error(f"Failed to parse title: {title}, Error: {e}")
                    return None

        async def parse_torrents(titles, max_concurrent_tasks):
            global sem
            sem = asyncio.Semaphore(max_concurrent_tasks)
            tasks = [parse_torrent(title) for title in titles]
            results = await asyncio.gather(*tasks, return_exceptions=True)
            return results

        async def process_batches(titles_batches, max_concurrent_tasks):
            results = []
            total_batches = len(titles_batches)
            for batch_number, titles in enumerate(titles_batches, start=1):
                batch_results = await parse_torrents(titles, max_concurrent_tasks)
                results.extend(batch_results)
                logger.info(f"Finished processing batch {batch_number}/{total_batches}")
            return results

        def run_process_batches(titles_batches, max_concurrent_tasks):
            return asyncio.run(process_batches(titles_batches, max_concurrent_tasks))

        ptt = Parser()
        add_defaults(ptt)
        logger.info("Parser initialized and defaults added.")
        """;

    public ParseTorrentNameService(ILogger<ParseTorrentNameService> logger)
    {
        _logger = logger;
        _initAsync = InitializePythonEngine();
    }

    public async Task StopPythonEngine()
    {
        await _initAsync;

        PythonEngine.EndAllowThreads(_mainThreadState);
        PythonEngine.Shutdown();
    }

    public async Task<List<TorrentInfo>> ParseAndPopulateAsync(List<ExtractedDmmEntry> torrents, int batchSize = 5000, int maxConcurrentTasks = 4)
    {
        await _initAsync;

        _logger.LogInformation("Parsett: Parsing {Count} torrents", torrents.Count);

        var titlesBatches = BatchTorrents(torrents.Select(x => x.Filename!).ToList(), batchSize).ToList();

        using (Py.GIL())
        {
            var results = _runProcessBatches(titlesBatches, maxConcurrentTasks);

            for (int i = 0; i < torrents.Count; i++)
            {
                var result = results[i];
                var torrent = torrents[i];
                var parsedResponse = ParseResult(result);

                if (parsedResponse.Success)
                {
                    parsedResponse.Response.InfoHash = torrent.InfoHash;
                    parsedResponse.Response.Size = torrent.Filesize;
                    parsedResponse.Response.RawTitle = torrent.Filename;

                    torrent.ParseResponse = parsedResponse.Response;
                }
            }
        }

        return torrents.Select(x => x.ParseResponse)
            .OfType<TorrentInfo>()
            .ToList();
    }

    private dynamic CreatePttParser()
    {
        using var gil = Py.GIL();
        using var scope = Py.CreateScope();
        scope.Exec(CreatePythonParserScript);

        dynamic runProcessBatches = scope.Get("run_process_batches");
        return runProcessBatches;
    }

    private ParseTorrentTitleResponse ParseResult(PyObject? result)
    {
        try
        {
            if (result == null)
            {
                return new ParseTorrentTitleResponse(false, null);
            }

            var torrentInfo = new TorrentInfo
            {
                Resolution = result.HasKey("resolution") ? result["resolution"].As<string>() : string.Empty,
                Remastered = result.HasKey("remastered") && result["remastered"].As<bool>(),
                Source = result.HasKey("source") ? result["source"].As<string>() : string.Empty,
                Codec = result.HasKey("codec") ? result["codec"].As<string>() : string.Empty,
                Group = result.HasKey("group") ? result["group"].As<string>() : string.Empty,
                Title = result.HasKey("title") ? result["title"].As<string>() : string.Empty,
                Episodes = result.HasKey("episodes") ? ConvertPyListToList<int>(result["episodes"]) : new List<int>(),
                Seasons = result.HasKey("seasons") ? ConvertPyListToList<int>(result["seasons"]) : new List<int>(),
                Languages = result.HasKey("languages") ? ConvertPyListToList<string>(result["languages"]) : new List<string>(),
                // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                Year = result.HasKey("year") ? result["year"].ToString() : string.Empty,
            };

            return new ParseTorrentTitleResponse(true, torrentInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while parsing result");
            return new ParseTorrentTitleResponse(false, null);
        }
    }

    private static List<TType> ConvertPyListToList<TType>(dynamic pyList)
    {
        var result = new List<TType>();
        foreach (var item in pyList)
        {
            using (Py.GIL())
            {
                result.Add(item.As<TType>());
            }
        }
        return result;
    }

    private static IEnumerable<List<string>> BatchTorrents(List<string> torrents, int batchSize)
    {
        for (int i = 0; i < torrents.Count; i += batchSize)
        {
            yield return torrents.GetRange(i, Math.Min(batchSize, torrents.Count - i));
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
            using (Py.GIL())
            {
                _sys = Py.Import("sys");
                _sys.path.append(Path.Combine(AppContext.BaseDirectory, "python"));
            }
            _runProcessBatches = CreatePttParser();
            _isInitialized = true;
            _logger.LogInformation("Parser initialized");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to initialize Python engine: {Message}", e.Message);
            Environment.Exit(1);
        }

        return Task.CompletedTask;
    }
}
