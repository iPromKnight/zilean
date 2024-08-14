using Zilean.Database.ModelConfiguration;

namespace Zilean.Database;

public class ZileanDbContext : DbContext
{
    public ZileanDbContext()
    {
    }

    public ZileanDbContext(DbContextOptions<ZileanDbContext> options): base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=zilean;Username=postgres;Password=postgres");
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TorrentInfoConfiguration());
        modelBuilder.ApplyConfiguration(new ImdbFileConfiguration());
    }

    public DbSet<TorrentInfo> Torrents => Set<TorrentInfo>();
    public DbSet<ImdbFile> ImdbFiles => Set<ImdbFile>();
}
