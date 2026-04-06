using Microsoft.EntityFrameworkCore;

// UserService owns the Users table and the UserServiceDB database.
// It does NOT own Orders or Books — those belong to OrderService.
public class UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserGuid);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
