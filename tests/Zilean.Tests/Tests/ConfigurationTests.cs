namespace Zilean.Tests.Tests;

public class ConfigurationTests
{
    private const string MockSettingsWithIngestion =
        """
        {
          "Zilean": {
            "Dmm": {
              "EnableScraping": true,
              "EnableEndpoint": true,
              "ScrapeSchedule": "0 * * * *",
              "MinimumReDownloadIntervalMinutes": 30,
              "MaxFilteredResults": 200,
              "MinimumScoreMatch": 0.85,
              "ImportBatched": false
            },
            "Torznab": {
              "EnableEndpoint": true
            },
            "Database": {
              "ConnectionString": "Host=localhost;Database=zilean;Username=postgres;Password=postgres;Include Error Detail=true;Timeout=300;CommandTimeout=300;"
            },
            "Torrents": {
              "EnableEndpoint": true
            },
            "Imdb": {
              "EnableImportMatching": true,
              "EnableEndpoint": true,
              "MinimumScoreMatch": 0.85
            },
            "Ingestion": {
              "ZurgInstances": [
                  {
                    "Url": "http://zurg:9999",
                    "EndpointType": 1
                  }
              ],
              "ZileanInstances": [],
              "EnableScraping": false,
              "Kubernetes": {
                "EnableServiceDiscovery": false,
                "KubernetesSelectors": [
                  {
                    "UrlTemplate": "http://zurg.{0}:9999",
                    "LabelSelector": "app.elfhosted.com/name=zurg",
                    "EndpointType": 1
                  }
                ],
                "KubeConfigFile": "/$HOME/.kube/config"
              },
              "BatchSize": 500,
              "MaxChannelSize": 5000,
              "ScrapeSchedule": "0 * * * *",
              "ZurgEndpointSuffix": "/debug/torrents",
              "ZileanEndpointSuffix": "/torrents/all"
            }
          }
        }
        """;

    [Fact]
    public void adds_json_configuration_file_to_builder_with_fake_filesystem_gets_ingestion_config()
    {
        // Arrange
        var testsFolder = CreateTestFolder();

        // Act
        var configuration =
            new ConfigurationBuilder()
                .SetBasePath(testsFolder)
                .AddJsonFile(ConfigurationLiterals.SettingsConfigFilename, false, false)
                .Build();

        var zileanConfig = configuration.GetZileanConfiguration();

        // Assert
        zileanConfig.Should().NotBeNull();

        // Dmm
        zileanConfig.Dmm.Should().NotBeNull();
        zileanConfig.Dmm.EnableScraping.Should().BeTrue();
        zileanConfig.Dmm.EnableEndpoint.Should().BeTrue();
        zileanConfig.Dmm.ScrapeSchedule.Should().Be("0 * * * *");
        zileanConfig.Dmm.MinimumReDownloadIntervalMinutes.Should().Be(30);
        zileanConfig.Dmm.MaxFilteredResults.Should().Be(200);
        zileanConfig.Dmm.MinimumScoreMatch.Should().Be(0.85);

        // Torznab
        zileanConfig.Torznab.Should().NotBeNull();
        zileanConfig.Torznab.EnableEndpoint.Should().BeTrue();

        // Database
        zileanConfig.Database.Should().NotBeNull();
        zileanConfig.Database.ConnectionString.Should()
            .Be(
                "Host=localhost;Database=zilean;Username=postgres;Password=postgres;Include Error Detail=true;Timeout=300;CommandTimeout=300;");

        // Torrents
        zileanConfig.Torrents.Should().NotBeNull();
        zileanConfig.Torrents.EnableEndpoint.Should().BeTrue();

        // Imdb
        zileanConfig.Imdb.Should().NotBeNull();
        zileanConfig.Imdb.EnableImportMatching.Should().BeTrue();
        zileanConfig.Imdb.EnableEndpoint.Should().BeTrue();
        zileanConfig.Imdb.MinimumScoreMatch.Should().Be(0.85);

        // Ingestion
        zileanConfig.Ingestion.Should().NotBeNull();
        zileanConfig.Ingestion.ZurgInstances.Should().HaveCount(1);
        zileanConfig.Ingestion.ZurgInstances[0].Url.Should().Be("http://zurg:9999");
        zileanConfig.Ingestion.ZurgInstances[0].EndpointType.Should().Be(GenericEndpointType.Zurg);
        zileanConfig.Ingestion.ZileanInstances.Should().BeEmpty();
        zileanConfig.Ingestion.EnableScraping.Should().BeFalse();
        zileanConfig.Ingestion.ScrapeSchedule.Should().Be("0 * * * *");
        zileanConfig.Ingestion.ZurgEndpointSuffix.Should().Be("/debug/torrents");
        zileanConfig.Ingestion.ZileanEndpointSuffix.Should().Be("/torrents/all");

        // Kubernetes
        zileanConfig.Ingestion.Kubernetes.Should().NotBeNull();
        zileanConfig.Ingestion.Kubernetes.EnableServiceDiscovery.Should().BeFalse();
        zileanConfig.Ingestion.Kubernetes.KubernetesSelectors.Should().HaveCount(1);
        zileanConfig.Ingestion.Kubernetes.KubernetesSelectors[0].UrlTemplate.Should().Be("http://zurg.{0}:9999");
        zileanConfig.Ingestion.Kubernetes.KubernetesSelectors[0].LabelSelector.Should().Be("app.elfhosted.com/name=zurg");
        zileanConfig.Ingestion.Kubernetes.KubernetesSelectors[0].EndpointType.Should().Be(GenericEndpointType.Zurg);
        zileanConfig.Ingestion.Kubernetes.KubeConfigFile.Should().Be("/$HOME/.kube/config");

        // Cleanup
        Directory.Delete(testsFolder, true);
    }

    [Fact]
    public void database_configuration_defaults_without_env_vars()
    {
        var savedVars = ClearDatabaseEnvVars();
        try
        {
            var dbConfig = new DatabaseConfiguration();

            dbConfig.ConnectionString.Should().NotBeNullOrWhiteSpace();
            dbConfig.ConnectionString.Should().Contain("Host=localhost");
            dbConfig.ConnectionString.Should().Contain("Database=zilean");
            dbConfig.ConnectionString.Should().Contain("Username=postgres");
        }
        finally
        {
            RestoreDatabaseEnvVars(savedVars);
        }
    }

    [Fact]
    public void database_configuration_respects_full_connection_string_env_var()
    {
        var savedVars = ClearDatabaseEnvVars();
        try
        {
            var expected = "Host=myhost;Database=mydb;Username=myuser;Password=mypass;";
            Environment.SetEnvironmentVariable("Zilean__Database__ConnectionString", expected);

            var dbConfig = new DatabaseConfiguration();

            dbConfig.ConnectionString.Should().Be(expected);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Zilean__Database__ConnectionString", null);
            RestoreDatabaseEnvVars(savedVars);
        }
    }

    [Fact]
    public void database_configuration_builds_from_individual_env_vars()
    {
        var savedVars = ClearDatabaseEnvVars();
        try
        {
            Environment.SetEnvironmentVariable("POSTGRES_HOST", "db.example.com");
            Environment.SetEnvironmentVariable("POSTGRES_PORT", "5433");
            Environment.SetEnvironmentVariable("POSTGRES_DB", "mydb");
            Environment.SetEnvironmentVariable("POSTGRES_USER", "admin");
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "secret");

            var dbConfig = new DatabaseConfiguration();

            dbConfig.ConnectionString.Should().Contain("Host=db.example.com");
            dbConfig.ConnectionString.Should().Contain("Port=5433");
            dbConfig.ConnectionString.Should().Contain("Database=mydb");
            dbConfig.ConnectionString.Should().Contain("Username=admin");
            dbConfig.ConnectionString.Should().Contain("Password=secret");
        }
        finally
        {
            RestoreDatabaseEnvVars(savedVars);
        }
    }

    [Fact]
    public void database_configuration_escapes_special_chars_in_password()
    {
        var savedVars = ClearDatabaseEnvVars();
        try
        {
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "p@ss#w0rd!&");

            var dbConfig = new DatabaseConfiguration();

            dbConfig.ConnectionString.Should().NotBeNullOrWhiteSpace();

            // Verify it round-trips correctly
            var parsed = new Npgsql.NpgsqlConnectionStringBuilder(dbConfig.ConnectionString);
            parsed.Password.Should().Be("p@ss#w0rd!&");
        }
        finally
        {
            RestoreDatabaseEnvVars(savedVars);
        }
    }

    [Fact]
    public void database_configuration_full_connection_string_takes_priority_over_individual_vars()
    {
        var savedVars = ClearDatabaseEnvVars();
        try
        {
            var expected = "Host=priority-host;Database=prioritydb;Username=user;Password=pass;";
            Environment.SetEnvironmentVariable("Zilean__Database__ConnectionString", expected);
            Environment.SetEnvironmentVariable("POSTGRES_HOST", "ignored-host");
            Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", "ignored-pass");

            var dbConfig = new DatabaseConfiguration();

            dbConfig.ConnectionString.Should().Be(expected);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Zilean__Database__ConnectionString", null);
            RestoreDatabaseEnvVars(savedVars);
        }
    }

    private static Dictionary<string, string?> ClearDatabaseEnvVars()
    {
        var vars = new[] { "POSTGRES_HOST", "POSTGRES_PORT", "POSTGRES_DB", "POSTGRES_USER", "POSTGRES_PASSWORD", "Zilean__Database__ConnectionString" };
        var saved = new Dictionary<string, string?>();
        foreach (var v in vars)
        {
            saved[v] = Environment.GetEnvironmentVariable(v);
            Environment.SetEnvironmentVariable(v, null);
        }
        return saved;
    }

    private static void RestoreDatabaseEnvVars(Dictionary<string, string?> saved)
    {
        foreach (var (key, value) in saved)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string CreateTestFolder()
    {
        var testsFolder = Path.Combine(Path.GetTempPath(), "Zilean.Tests");

        try
        {
            if (Directory.Exists(testsFolder))
            {
                Directory.Delete(testsFolder, true);
            }

            Directory.CreateDirectory(testsFolder);

            var configFile = Path.Combine(testsFolder, ConfigurationLiterals.SettingsConfigFilename);
            File.WriteAllText(configFile, MockSettingsWithIngestion);

            return testsFolder;
        }
        catch (Exception)
        {
            Directory.Delete(testsFolder, true);
            throw;
        }
    }
}
