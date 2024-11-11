global using System.Collections.Concurrent;
global using System.Data;
global using System.Diagnostics;
global using Dapper;
global using EFCore.BulkExtensions;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Npgsql;
global using NpgsqlTypes;
global using Spectre.Console;
global using Zilean.Database.Dtos;
global using Zilean.Database.ModelConfiguration;
global using Zilean.Shared.Features.Configuration;
global using Zilean.Shared.Features.Dmm;
global using Zilean.Shared.Features.Imdb;
global using Zilean.Shared.Features.Statistics;