namespace Zilean.Database.ModelConfiguration;

public class ImdbFileConfiguration:  IEntityTypeConfiguration<ImdbFile>
{
    public void Configure(EntityTypeBuilder<ImdbFile> builder)
    {
        builder.ToTable("ImdbFiles");

        builder.HasKey(i => i.ImdbId);

        builder.Property(i => i.ImdbId)
            .HasColumnType("text");

        builder.Property(i => i.Category)
            .HasColumnType("text");

        builder.Property(i => i.Title)
            .HasColumnType("text");

        builder.Property(i => i.Adult)
            .HasColumnType("boolean");

        builder.Property(i => i.Year)
            .HasColumnType("integer");

        builder.HasIndex(i => i.ImdbId)
            .IsUnique();
    }
}
