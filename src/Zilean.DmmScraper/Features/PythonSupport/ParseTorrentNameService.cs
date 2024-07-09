namespace Zilean.DmmScraper.Features.PythonSupport;

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
        import sys
        import os
        from contextlib import contextmanager
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

        @contextmanager
        def suppress_print():
            original_stdout = sys.stdout
            original_stderr = sys.stderr
            sys.stdout = open(os.devnull, 'w')
            sys.stderr = open(os.devnull, 'w')
            try:
                yield
            finally:
                sys.stdout.close()
                sys.stderr.close()
                sys.stdout = original_stdout
                sys.stderr = original_stderr

        async def parse_torrent(info):
            async with sem:
                title, info_hash = info
                try:
                    with suppress_print():
                        result = ptt.parse(title)
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
            for batch_number, infos in enumerate(info_batches, start=1):
                batch_results = await parse_torrents(infos, max_concurrent_tasks)
                results.extend(batch_results)
                logger.info(f"Finished processing batch {light_green}{batch_number}{reset} / {light_green}{total_batches}{reset}")
            return results

        def run_process_batches(info_batches, max_concurrent_tasks):
            return asyncio.run(process_batches(info_batches, max_concurrent_tasks))

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

        _mainThreadState = PythonEngine.BeginAllowThreads();
        PythonEngine.EndAllowThreads(_mainThreadState);

        _runProcessBatches.Dispose();
        _sys.Dispose();

        PythonEngine.Shutdown();

        _isInitialized = false;
    }

    public async Task<List<TorrentInfo>> ParseAndPopulateAsync(List<ExtractedDmmEntry> torrents, int batchSize = 5000)
    {
        await _initAsync;

        _logger.LogInformation("Parsett: Parsing {Count} torrents", torrents.Count);

         var torrentDict = torrents.ToDictionary(t => t.InfoHash!, t => t);

        var infoBatches = torrents
            .Select(x => new List<object> { x.Filename!, x.InfoHash! })
            .ToList()
            .ToChunks(batchSize)
            .ToList();

        _mainThreadState = PythonEngine.BeginAllowThreads();
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
        PythonEngine.EndAllowThreads(_mainThreadState);

        return torrents.Select(x => x.ParseResponse)
            .OfType<TorrentInfo>()
            .ToList();
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
                Episodes = result.HasKey("episodes") ? ConvertPyListToList<int>(result["episodes"]) : [],
                Seasons = result.HasKey("seasons") ? ConvertPyListToList<int>(result["seasons"]) : [],
                Languages = result.HasKey("languages") ? ConvertPyListToList<string>(result["languages"]) : [],
            };

            torrentInfo.IsPossibleMovie = torrentInfo.Episodes.Count == 0 && torrentInfo.Seasons.Count == 0;

            ConvertYear(result, torrentInfo);

            result.Dispose();

            return new ParseTorrentTitleResponse(true, torrentInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while parsing result");
            return new ParseTorrentTitleResponse(false, null);
        }
    }

    private static void ConvertYear(PyObject result, TorrentInfo torrentInfo)
    {
        try
        {
            torrentInfo.Year = result.HasKey("year") ? result["year"].As<int>() : 0;
        }
        catch
        {
           // ignore
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
            RuntimeData.FormatterType = typeof(NoopFormatter);
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                _sys = Py.Import("sys");
                _sys.path.append(Path.Combine(AppContext.BaseDirectory, "python"));
            }
            _runProcessBatches = CreatePttParser();
            _isInitialized = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to initialize Python engine: {Message}", e.Message);
            Environment.Exit(1);
        }

        return Task.CompletedTask;
    }

    private static dynamic CreatePttParser()
    {
        using var gil = Py.GIL();
        using var scope = Py.CreateScope();
        scope.Exec(CreatePythonParserScript);

        dynamic runProcessBatches = scope.Get("run_process_batches");
        return runProcessBatches;
    }
}
