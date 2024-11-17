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
        zileanConfig.Dmm.ImportBatched.Should().BeFalse();

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
        zileanConfig.Ingestion.BatchSize.Should().Be(500);
        zileanConfig.Ingestion.MaxChannelSize.Should().Be(5000);
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
