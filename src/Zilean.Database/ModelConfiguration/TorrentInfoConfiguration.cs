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

        builder.Property(t => t.Codec)
            .HasColumnType("text[]")
            .HasAnnotation("Relational:JsonPropertyName", "codec");

        builder.Property(t => t.Audio)
            .HasColumnType("text[]")
            .HasAnnotation("Relational:JsonPropertyName", "audio");

        builder.Property(t => t.Quality)
            .HasColumnType("text[]")
            .HasAnnotation("Relational:JsonPropertyName", "quality");

        builder.Property(t => t.Episodes)
            .HasColumnType("integer[]")
            .HasAnnotation("Relational:JsonPropertyName", "episodes");

        builder.Property(t => t.Languages)
            .HasColumnType("text[]")
            .HasAnnotation("Relational:JsonPropertyName", "languages");

        builder.Property(t => t.RawTitle)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "raw_title");

        builder.Property(t => t.Remastered)
            .HasColumnType("boolean")
            .HasAnnotation("Relational:JsonPropertyName", "remastered");

        builder.Property(t => t.Resolution)
            .HasColumnType("text[]")
            .HasAnnotation("Relational:JsonPropertyName", "resolution");

        builder.Property(t => t.Seasons)
            .HasColumnType("integer[]")
            .HasAnnotation("Relational:JsonPropertyName", "seasons");

        builder.Property(t => t.Size)
            .HasColumnType("bigint")
            .HasAnnotation("Relational:JsonPropertyName", "size");

        builder.Property(t => t.Title)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "parsed_title");

        builder.Property(t => t.Year)
            .HasColumnType("integer")
            .HasAnnotation("Relational:JsonPropertyName", "year");

        builder.HasOne(t => t.Imdb)
            .WithMany()
            .HasForeignKey(t => t.ImdbId);

        builder.HasIndex(t => t.ImdbId);

        builder.HasIndex(t => t.InfoHash)
            .IsUnique();
    }
}
