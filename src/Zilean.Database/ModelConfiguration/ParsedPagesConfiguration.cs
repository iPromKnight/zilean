namespace Zilean.Database.ModelConfiguration;

public class ParsedPagesConfiguration : IEntityTypeConfiguration<ParsedPages>
{
    public void Configure(EntityTypeBuilder<ParsedPages> builder)
    {
        builder.ToTable("ParsedPages");

        builder.HasKey(x => x.Page);
        builder.Property(x => x.Page).IsRequired();
        builder.Property(x => x.EntryCount).IsRequired();
    }
}
