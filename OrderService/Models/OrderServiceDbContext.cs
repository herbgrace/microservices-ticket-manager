using Microsoft.EntityFrameworkCore;

// OrderService is the sole owner of OrderServiceDB.
// It manages the Orders and Books tables only — no Users table.
// UserGuid on Order is a soft reference (plain Guid, no FK constraint) to UserService's database.
public class OrderServiceDbContext(DbContextOptions<OrderServiceDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Explicitly map Book.OrderGuid as the FK for the Book→Order relationship.
        // Without this, EF creates a shadow column "OrderGuid1" because it can't
        // automatically match the existing OrderGuid property to the navigation property.
        modelBuilder.Entity<Ticket>()
            .HasOne(b => b.Order)
            .WithMany(o => o.Tickets)
            .HasForeignKey(b => b.OrderGuid);

        // Explicit decimal precision prevents silent truncation of book prices.
        modelBuilder.Entity<Ticket>()
            .Property(b => b.Price)
            .HasPrecision(18, 2);
    }
}