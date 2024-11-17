namespace Zilean.Database.ModelConfiguration;

public class BlacklistedItemConfiguration: IEntityTypeConfiguration<BlacklistedItem>
{
    public void Configure(EntityTypeBuilder<BlacklistedItem> builder)
    {
        builder.ToTable("BlacklistedItems");

        builder.HasKey(i => i.InfoHash);

        builder.Property(i => i.InfoHash)
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "info_hash");

        builder.Property(i => i.Reason)
            .IsRequired()
            .HasColumnType("text")
            .HasAnnotation("Relational:JsonPropertyName", "reason");

        builder.Property(t => t.BlacklistedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now() at time zone 'utc'")
            .HasAnnotation("Relational:JsonPropertyName", "blacklisted_at");

        builder.HasIndex(i => i.InfoHash)
            .IsUnique();
    }
}
