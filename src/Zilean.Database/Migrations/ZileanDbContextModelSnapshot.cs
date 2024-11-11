﻿// <auto-generated />
using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Zilean.Database;

#nullable disable

namespace Zilean.Database.Migrations
{
    [DbContext(typeof(ZileanDbContext))]
    partial class ZileanDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Zilean.Shared.Features.Dmm.ParsedPages", b =>
                {
                    b.Property<string>("Page")
                        .HasColumnType("text");

                    b.Property<int>("EntryCount")
                        .HasColumnType("integer");

                    b.HasKey("Page");

                    b.ToTable("ParsedPages", (string)null);
                });

            modelBuilder.Entity("Zilean.Shared.Features.Dmm.TorrentInfo", b =>
                {
                    b.Property<string>("InfoHash")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "info_hash");

                    b.Property<string[]>("Audio")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasAnnotation("Relational:JsonPropertyName", "audio");

                    b.Property<string>("BitDepth")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "bit_depth");

                    b.Property<string>("Bitrate")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "bitrate");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "category");

                    b.Property<string[]>("Channels")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasAnnotation("Relational:JsonPropertyName", "channels");

                    b.Property<string>("Codec")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "codec");

                    b.Property<bool?>("Complete")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "complete");

                    b.Property<string>("Container")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "container");

                    b.Property<bool?>("Converted")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "converted");

                    b.Property<string>("Country")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "country");

                    b.Property<string>("Date")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "date");

                    b.Property<bool?>("Documentary")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "documentary");

                    b.Property<bool?>("Dubbed")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "dubbed");

                    b.Property<string>("Edition")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "edition");

                    b.Property<string>("EpisodeCode")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "episode_code");

                    b.Property<int[]>("Episodes")
                        .IsRequired()
                        .HasColumnType("integer[]")
                        .HasAnnotation("Relational:JsonPropertyName", "episodes");

                    b.Property<bool?>("Extended")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "extended");

                    b.Property<string>("Extension")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "extension");

                    b.Property<string>("Group")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "group");

                    b.Property<bool?>("Hardcoded")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "hardcoded");

                    b.Property<string[]>("Hdr")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasAnnotation("Relational:JsonPropertyName", "hdr");

                    b.Property<string>("ImdbId")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "imdb_id");

                    b.Property<bool?>("Is3d")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "_3d");

                    b.Property<string[]>("Languages")
                        .IsRequired()
                        .HasColumnType("text[]")
                        .HasAnnotation("Relational:JsonPropertyName", "languages");

                    b.Property<string>("Network")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "network");

                    b.Property<string>("NormalizedTitle")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "normalized_title");

                    b.Property<string>("ParsedTitle")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "parsed_title");

                    b.Property<bool?>("Ppv")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "ppv");

                    b.Property<bool?>("Proper")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "proper");

                    b.Property<string>("Quality")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "quality");

                    b.Property<string>("RawTitle")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "raw_title");

                    b.Property<string>("Region")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "region");

                    b.Property<bool?>("Remastered")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "remastered");

                    b.Property<bool?>("Repack")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "repack");

                    b.Property<string>("Resolution")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "resolution");

                    b.Property<bool?>("Retail")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "retail");

                    b.Property<int[]>("Seasons")
                        .IsRequired()
                        .HasColumnType("integer[]")
                        .HasAnnotation("Relational:JsonPropertyName", "seasons");

                    b.Property<string>("Site")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "site");

                    b.Property<string>("Size")
                        .HasColumnType("text")
                        .HasAnnotation("Relational:JsonPropertyName", "size");

                    b.Property<bool?>("Subbed")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "subbed");

                    b.Property<bool?>("Torrent")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "torrent");

                    b.Property<bool?>("Trash")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "trash");

                    b.Property<bool?>("Unrated")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "unrated");

                    b.Property<bool?>("Upscaled")
                        .IsRequired()
                        .HasColumnType("boolean")
                        .HasAnnotation("Relational:JsonPropertyName", "upscaled");

                    b.Property<int[]>("Volumes")
                        .IsRequired()
                        .HasColumnType("integer[]")
                        .HasAnnotation("Relational:JsonPropertyName", "volumes");

                    b.Property<int?>("Year")
                        .HasColumnType("integer")
                        .HasAnnotation("Relational:JsonPropertyName", "year");

                    b.HasKey("InfoHash");

                    b.HasIndex("ImdbId");

                    b.HasIndex("InfoHash")
                        .IsUnique();

                    b.ToTable("Torrents", (string)null);
                });

            modelBuilder.Entity("Zilean.Shared.Features.Imdb.ImdbFile", b =>
                {
                    b.Property<string>("ImdbId")
                        .HasColumnType("text");

                    b.Property<bool>("Adult")
                        .HasColumnType("boolean");

                    b.Property<string>("Category")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<int>("Year")
                        .HasColumnType("integer");

                    b.HasKey("ImdbId");

                    b.HasIndex("ImdbId")
                        .IsUnique();

                    b.ToTable("ImdbFiles", (string)null);

                    b.HasAnnotation("Relational:JsonPropertyName", "imdb");
                });

            modelBuilder.Entity("Zilean.Shared.Features.Statistics.ImportMetadata", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("text");

                    b.Property<JsonDocument>("Value")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.HasKey("Key");

                    b.ToTable("ImportMetadata");
                });

            modelBuilder.Entity("Zilean.Shared.Features.Dmm.TorrentInfo", b =>
                {
                    b.HasOne("Zilean.Shared.Features.Imdb.ImdbFile", "Imdb")
                        .WithMany()
                        .HasForeignKey("ImdbId");

                    b.Navigation("Imdb");
                });
#pragma warning restore 612, 618
        }
    }
}