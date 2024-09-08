using Microsoft.EntityFrameworkCore;

namespace Stock.API.Models.Context;

public class StockDbContext : DbContext
{
    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<OrderInbox> OrderInboxes { get; set; }
}