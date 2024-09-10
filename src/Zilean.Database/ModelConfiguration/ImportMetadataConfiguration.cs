namespace Zilean.Database.ModelConfiguration;

public class ImportMetadataConfiguration : IEntityTypeConfiguration<ImportMetadata>
{
    public void Configure(EntityTypeBuilder<ImportMetadata> builder)
    {
        builder.ToTable("ImportMetadata");

        builder.HasKey(x => x.Key);

        builder.Property(e => e.Value)
            .IsRequired()
            .HasColumnType("jsonb");
    }
}
