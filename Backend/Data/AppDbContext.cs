using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Irasas> Irasas { get; set; }
    public DbSet<Naudotojas> Naudotojas { get; set; }
    public DbSet<IrasasNaudotojas> IrasasNaudotojas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IrasasNaudotojas>()
            .HasKey(x => new { x.IrasasId, x.NaudotojasId });

        modelBuilder.Entity<IrasasNaudotojas>()
            .HasOne(x => x.Irasas)
            .WithMany(x => x.Naudotojai)
            .HasForeignKey(x => x.IrasasId);

        modelBuilder.Entity<IrasasNaudotojas>()
            .HasOne(x => x.Naudotojas)
            .WithMany(x => x.Irasai)
            .HasForeignKey(x => x.NaudotojasId);

        modelBuilder.Entity<Irasas>()
            .Property(x => x.Dienos_pries)
            .HasDefaultValue(0);

        modelBuilder.Entity<Irasas>()
            .Property(x => x.Dienu_daznumas)
            .HasDefaultValue(0);

        modelBuilder.Entity<Irasas>()
            .Property(x => x.Archyvuotas)
            .HasDefaultValue(false);

        modelBuilder.Entity<Naudotojas>()
            .Property(x => x.Adminas)
            .HasDefaultValue(false);

        modelBuilder.Entity<IrasasNaudotojas>()
            .Property(x => x.Prekes_Adminas)
            .HasDefaultValue(false);
    }

}