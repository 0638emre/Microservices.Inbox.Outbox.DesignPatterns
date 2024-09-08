using Microsoft.EntityFrameworkCore;

namespace Order.API.Models.Context;

public class OrderAPIDBContext : DbContext
{
    public OrderAPIDBContext(DbContextOptions<OrderAPIDBContext> options) : base(options)
    {
        
    }
    
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderOutbox> OrderOutbox { get; set; }
}