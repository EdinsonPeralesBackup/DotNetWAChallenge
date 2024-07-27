namespace CachedInventory;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public interface IOperationsTracker
{
  Task<int[]> GetActionsByProductId(int productId);
  Task<int> CreateOperationsTracker(int productId,int action);
  Task FailUpdateByOperationId(int operationId);
  Task RemoveCache();
}
public class OperationsTracker : IOperationsTracker
{
  private readonly ApplicationDbContext context;
  public OperationsTracker(ApplicationDbContext context) => this.context = context;

  public async Task<int[]> GetActionsByProductId(int productId) => await context.OperationTracker
              .Where(op => op.ProductId == productId && op.Ok && op.InCache)
              .Select(op => op.Action)
              .ToArrayAsync();

  public async Task<int> CreateOperationsTracker(int productId, int action)
  {
    var newOperation = new Operation
    {
        ProductId = productId,
        Action = action
    };
    context.OperationTracker.Add(newOperation);
    await context.SaveChangesAsync();
    return newOperation.Id;
  }
  public async Task FailUpdateByOperationId(int operationId)
  {
    var operation = await context.OperationTracker
        .FirstOrDefaultAsync(op => op.Id == operationId);

    if (operation != null)
    {
        operation.Ok = false;
        await context.SaveChangesAsync();
    }
  }

  public async Task RemoveCache()
  {
    var operations = await context.OperationTracker.ToListAsync();

    foreach (var operation in operations)
    {
        operation.InCache = false;
    }

    await context.SaveChangesAsync();
  }
}
