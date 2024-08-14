namespace Zilean.DmmScraper.Features.PythonSupport;

public class ParseTorrentNameService
{
    private readonly Task _initAsync;
    // ReSharper disable once NotAccessedField.Local
    private IntPtr _mainThreadState;
    private bool _isInitialized;
    private dynamic? _sys;
    private dynamic? _runProcessBatches;
    private dynamic? _runTitleMatch;
    private readonly ILogger<ParseTorrentNameService> _logger;

    private const string ParserScript =
        """
        from RTN import parse, title_match
        import asyncio
        from rich.progress import Progress, TaskID
        from loguru import logger

        light_blue = "\033[38;2;0;175;255m"
        light_green = "\033[38;2;172;233;149m"
        reset = "\033[0m"

        custom_format = (
            "[{time:HH:mm:ss}] | "
            f"{light_blue}{{level}}{reset} | "
            f"{light_green}\"Zilean.DmmScraper.Features.Python.ParseTorrentNameService\"{reset} | "
            "{message}"
        )

        logger.remove()
        logger.add(lambda msg: print(msg, end=''), format=custom_format, colorize=True)

        sem = None

        async def parse_torrent(info):
            async with sem:
                title, info_hash = info
                try:
                    result = parse(title)
                    return {'infoHash': info_hash, 'result': result}
                except Exception as e:
                    logger.error(f"Failed to parse title: {title}, Error: {e}")
                    return {'infoHash': info_hash, 'result': None, 'error': str(e)}

        async def parse_torrents(infos, max_concurrent_tasks):
            global sem
            sem = asyncio.Semaphore(max_concurrent_tasks)
            tasks = [parse_torrent(info) for info in infos]
            results = await asyncio.gather(*tasks, return_exceptions=True)
            return results

        async def process_batches(info_batches, max_concurrent_tasks):
            results = []
            total_batches = len(info_batches)

            with Progress() as progress:
                task_id = progress.add_task("[green]Processing batches...", total=total_batches)

                for batch_number, infos in enumerate(info_batches, start=1):
                    batch_results = await parse_torrents(infos, max_concurrent_tasks)
                    results.extend(batch_results)
                    progress.update(task_id, advance=1)

                progress.remove_task(task_id)

            return results

        def run_title_match(title1, title2):
            try:
                return title_match(title1, title2)
            except Exception as e:
                logger.error(f"Failed to match titles: {title1}, {title2}, Error: {e}")
                return False

        def run_process_batches(info_batches, max_concurrent_tasks):
            return asyncio.run(process_batches(info_batches, max_concurrent_tasks))

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

        _runProcessBatches.Dispose();
        _runTitleMatch.Dispose();
        _sys.Dispose();

        PythonEngine.Shutdown();

        _isInitialized = false;
    }

    public async Task<List<TorrentInfo>> ParseAndPopulateAsync(List<ExtractedDmmEntry> torrents, int batchSize = 5000)
    {
        await _initAsync;

        _logger.LogInformation("RTN: Parsing {Count} torrents", torrents.Count);

        var torrentDict = torrents.ToDictionary(t => t.InfoHash!, t => t);

        var infoBatches = torrents
            .Select(x => new List<object> { x.Filename!, x.InfoHash! })
            .ToList()
            .ToChunks(batchSize)
            .ToList();

        using (Py.GIL())
        {
            var results = _runProcessBatches(infoBatches, Environment.ProcessorCount);

            foreach (var result in results)
            {
                var infoHash = result["infoHash"].As<string>();
                var parsedResult = result["result"];

                var torrent = torrentDict[infoHash];

                if (torrent is null)
                {
                    continue;
                }

                ParseTorrentTitleResponse parsedResponse = ParseResult(parsedResult);

                if (parsedResponse.Success)
                {
                    parsedResponse.Response.InfoHash = torrent.InfoHash;
                    parsedResponse.Response.Size = torrent.Filesize;
                    parsedResponse.Response.RawTitle = torrent.Filename;
                    torrent.ParseResponse = parsedResponse.Response;
                }
            }

            results.Dispose();
        }

        return torrents.Select(x => x.ParseResponse)
            .OfType<TorrentInfo>()
            .ToList();
    }

    public async Task<bool> TitleMatch(string title1, string title2)
    {
        await _initAsync;

        using (Py.GIL())
        {
            try
            {
                return _runTitleMatch(title1, title2).As<bool>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running title match");
                return false;
            }
        }
    }

    private ParseTorrentTitleResponse ParseResult(dynamic? result)
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

            var torrentInfo = JsonSerializer.Deserialize<TorrentInfo>(json);

            torrentInfo.Category = mediaType.Equals("movie", StringComparison.OrdinalIgnoreCase) ? "movie" : "tvSeries";

            result.Dispose();

            return new ParseTorrentTitleResponse(true, torrentInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while parsing result");
            return new ParseTorrentTitleResponse(false, null);
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var pathToVirtualEnv = Environment.GetEnvironmentVariable("ZILEAN_PYTHON_VENV") ?? string.Empty;
                if (string.IsNullOrWhiteSpace(pathToVirtualEnv))
                {
                    _logger.LogWarning("`ZILEAN_PYTHON_VENV` env is not set. Exiting Application");
                    Environment.Exit(1);
                    return Task.CompletedTask;
                }

                var path = Environment.GetEnvironmentVariable("PATH").TrimEnd(';');
                path = string.IsNullOrEmpty(path) ? pathToVirtualEnv : path + ";" + pathToVirtualEnv;
                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("PATH", pathToVirtualEnv, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("PYTHONPATH", $@"{pathToVirtualEnv}\Lib\site-packages;{pathToVirtualEnv}\Lib", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("ZILEAN_PYTHON_PYLIB", $@"{pathToVirtualEnv}\python311.dll", EnvironmentVariableTarget.Process);
            }

            var pythonDllEnv = Environment.GetEnvironmentVariable("ZILEAN_PYTHON_PYLIB");

            if (string.IsNullOrWhiteSpace(pythonDllEnv))
            {
                _logger.LogWarning("`ZILEAN_PYTHON_PYLIB` env is not set. Exiting Application");
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
            CreateParserScript();
            _isInitialized = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to initialize Python engine: {Message}", e.Message);
            Environment.Exit(1);
        }

        return Task.CompletedTask;
    }

    private void CreateParserScript()
    {
        using var gil = Py.GIL();
        using var scope = Py.CreateScope();
        scope.Exec(ParserScript);

        _runProcessBatches = scope.Get("run_process_batches");
        _runTitleMatch = scope.Get("run_title_match");
    }
}
