namespace CachedInventory;

using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
  {
  }

  public DbSet<Operation> OperationTracker => Set<Operation>();
}

public class Operation
{
  public int Id { get; set; }
  public DateTime Time { get; set; } = DateTime.Now;
  public bool Ok { get; set; } = true;
  public int ProductId { get; set; }
  public int Action { get; set; }
  public bool InCache { get; set; } = true;
}
