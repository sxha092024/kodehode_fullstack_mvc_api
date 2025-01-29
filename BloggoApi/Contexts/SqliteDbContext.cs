using BloggoApi.Contexts.Converters;
using BloggoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace BloggoApi.Contexts;

// UUIDv7 keys have the very nice property of when sorted, they are also ordered by creation time
// Unfortunately, with the relative freshness of the standard, there are few DBs or frameworks which
// natively support it in their generators / value providers
internal class V7Generator : ValueGenerator<Guid>
{
    public override bool GeneratesTemporaryValues => false;

    public override Guid Next(EntityEntry entry)
    {
        return Guid.CreateVersion7();
    }
}

public class SqliteDbContext : DbContext
{
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Database/Bloggo.db; foreign keys=true");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Property(u => u.UserId).HasValueGenerator(typeof(V7Generator));
        // exceedingly annoying but this is the way in which efcore defines a unique constraint on the table side
        modelBuilder.Entity<User>().HasAlternateKey(u => u.UserName);
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.OwnedPosts)
            .WithOne(b => b.Owner)
            .HasForeignKey(b => b.OwnerId)
            .IsRequired();
        modelBuilder.Entity<User>().HasMany(u => u.AuthoredPosts).WithMany(b => b.Authors);

        modelBuilder
            .Entity<BlogPost>()
            .Property(b => b.OwnerId)
            .HasValueGenerator(typeof(V7Generator));
        modelBuilder.Entity<BlogPost>()
            .Property(b => b.CreatedAt)
            .ValueGeneratedOnAdd()
            // NOTE: sqlite specific syntax, default value on insert is current time aligned to UTC-0
            .HasDefaultValueSql("strftime('%FT%H:%M:%fZ+00:00', 'now')");
        modelBuilder
            .Entity<BlogPost>()
            .HasOne(b => b.Owner)
            .WithMany(u => u.OwnedPosts)
            .HasForeignKey(b => b.OwnerId)
            .IsRequired();
        modelBuilder.Entity<BlogPost>().HasMany(b => b.Authors).WithMany(u => u.AuthoredPosts);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeAsUtcValueConverter>();
        configurationBuilder
            .Properties<DateTime?>()
            .HaveConversion<NullableDateTimeAsUtcValueConverter>();
    }
}
