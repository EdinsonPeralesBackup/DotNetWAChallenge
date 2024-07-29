namespace CachedInventory;

using Microsoft.AspNetCore.Mvc;

public static class CachedInventoryApiBuilder
{
  public static WebApplication Build(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddScoped<IWarehouseStockSystemClient, WarehouseStockSystemClient>();
    builder.Services.AddScoped<ICache, Cache>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.MapGet(
        "/stock/{productId:int}",
        async ([FromServices] IWarehouseStockSystemClient client, [FromServices] ICache cache, int productId) =>
          await GetStockWithCache(client,cache,productId))
      .WithName("GetStock")
      .WithOpenApi();

    app.MapPost(
        "/stock/retrieve",
        async ([FromServices] IWarehouseStockSystemClient client,[FromServices] ICache cache ,[FromBody] RetrieveStockRequest req) =>
        {
          var stock = await GetStockWithCache(client,cache,req.ProductId);
          if (stock < req.Amount)
          {
            return Results.BadRequest("Not enough stock.");
          }

          _=Task.Run(() => client.UpdateStock(req.ProductId, stock - req.Amount));
          cache.AddOrUpdateStock(req.ProductId,stock-req.Amount);
          return Results.Ok();
        })
      .WithName("RetrieveStock")
      .WithOpenApi();


    app.MapPost(
        "/stock/restock",
        async ([FromServices] IWarehouseStockSystemClient client,[FromServices] ICache cache,[FromBody] RestockRequest req) =>
        {
          var stock = await GetStockWithCache(client,cache,req.ProductId);
          _= Task.Run(() => client.UpdateStock(req.ProductId, req.Amount + stock));
          return Results.Ok();
        })
      .WithName("Restock")
      .WithOpenApi();

    return app;
  }
  public static async Task<int> GetStockWithCache(
  IWarehouseStockSystemClient client,
  ICache cache,
  int productId)
  {
    if (cache.InCache(productId))
    {
      return cache.GetStock(productId);
    }
    else
    {
      var stock = await client.GetStock(productId);
      cache.AddOrUpdateStock(productId,stock);
      return stock;
    }
  }
}

public record RetrieveStockRequest(int ProductId, int Amount);

public record RestockRequest(int ProductId, int Amount);
