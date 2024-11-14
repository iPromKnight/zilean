namespace Zilean.Database.ModelConfiguration;

public class TorrentInfoConfiguration : IEntityTypeConfiguration<TorrentInfo>
{
    public void Configure(EntityTypeBuilder<TorrentInfo> builder)
    {
        builder.ToTable("Torrents");

        builder.HasKey(t => t.InfoHash);

        builder.Property(t => t.InfoHash)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "info_hash");

        builder.Property(t => t.Category)
            .IsRequired()
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "category");

        builder.Property(t => t.RawTitle)
            .IsRequired()
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "raw_title");

        builder.Property(t => t.ParsedTitle)
            .IsRequired()
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "parsed_title");

        builder.Property(t => t.NormalizedTitle)
            .IsRequired()
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "normalized_title");

        builder.Property(t => t.Trash)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "trash");

        builder.Property(t => t.Year)
            .HasColumnType("integer")
            .HasAnnotation("Relational:JsonPropertyName", "year");

        builder.Property(t => t.Resolution)
            .IsRequired()
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "resolution");

        builder.Property(t => t.Seasons)
            .IsRequired()
            .HasColumnType("integer[]")
            .HasAnnotation("Relational:JsonPropertyName", "seasons");

        builder.Property(t => t.Episodes)
            .IsRequired()
            .HasColumnType("integer[]")
            .HasAnnotation("Relational:JsonPropertyName", "episodes");

        builder.Property(t => t.Complete)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "complete");

        builder.Property(t => t.Volumes)
            .IsRequired()
            .HasColumnType("integer[]")
            .HasAnnotation("Relational:JsonPropertyName", "volumes");

        builder.Property(t => t.Languages)
            .IsRequired()
            .HasColumnType("text[]")
            .HasAnnotation("Relational:JsonPropertyName", "languages");

        builder.Property(t => t.Quality)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "quality");

        builder.Property(t => t.Hdr)
            .IsRequired()
            .HasColumnType("text[]")
            .HasAnnotation("Relational:JsonPropertyName", "hdr");

        builder.Property(t => t.Codec)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "codec");

        builder.Property(t => t.Audio)
            .IsRequired()
            .HasColumnType("text[]")
            .HasAnnotation("Relational:JsonPropertyName", "audio");

        builder.Property(t => t.Channels)
            .IsRequired()
            .HasColumnType("text[]")
            .HasAnnotation("Relational:JsonPropertyName", "channels");

        builder.Property(t => t.Dubbed)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "dubbed");

        builder.Property(t => t.Subbed)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "subbed");

        builder.Property(t => t.Date)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "date");

        builder.Property(t => t.Group)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "group");

        builder.Property(t => t.Edition)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "edition");

        builder.Property(t => t.BitDepth)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "bit_depth");

        builder.Property(t => t.Bitrate)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "bitrate");

        builder.Property(t => t.Network)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "network");

        builder.Property(t => t.Extended)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "extended");

        builder.Property(t => t.Converted)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "converted");

        builder.Property(t => t.Hardcoded)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "hardcoded");

        builder.Property(t => t.Region)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "region");

        builder.Property(t => t.Ppv)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "ppv");

        builder.Property(t => t.Is3d)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "_3d");

        builder.Property(t => t.Site)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "site");

        builder.Property(t => t.Size)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "size");

        builder.Property(t => t.Proper)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "proper");

        builder.Property(t => t.Repack)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "repack");

        builder.Property(t => t.Retail)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "retail");

        builder.Property(t => t.Upscaled)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "upscaled");

        builder.Property(t => t.Remastered)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "remastered");

        builder.Property(t => t.Unrated)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "unrated");

        builder.Property(t => t.Documentary)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "documentary");

        builder.Property(t => t.EpisodeCode)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "episode_code");

        builder.Property(t => t.Country)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "country");

        builder.Property(t => t.Container)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "container");

        builder.Property(t => t.Extension)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "extension");

        builder.Property(t => t.Torrent)
            .IsRequired()
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "torrent");

        builder.HasOne(t => t.Imdb)
            .WithMany()
            .HasForeignKey(t => t.ImdbId);

        builder.Property(t => t.IngestedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now() at time zone 'utc'")
            .HasAnnotation("Relational:JsonPropertyName", "ingested_at");

        builder.HasIndex(t => t.ImdbId);

        builder.HasIndex(t => t.InfoHash)
            .IsUnique();
    }
}
