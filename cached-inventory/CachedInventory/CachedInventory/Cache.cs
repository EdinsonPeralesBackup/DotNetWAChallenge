namespace CachedInventory;
using System.Collections.Concurrent;

public interface ICache
{
  int GetStock(int productId);
  void AddOrUpdateStock(int productId, int newAmount);
  bool InCache(int productId);
}

public class Cache : ICache
{
  private readonly ConcurrentDictionary<int, int> dictionary = new();
  public int GetStock(int productId)
  {
    _ = dictionary.TryGetValue(productId, out var value);
    return value;
  }
  public void AddOrUpdateStock(int productId, int newAmount) => dictionary.AddOrUpdate(productId, newAmount, (k, oldValue) => newAmount);

  public bool InCache(int productId) => dictionary.TryGetValue(productId, out var _);
}
