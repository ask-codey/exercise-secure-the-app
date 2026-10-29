using NorthwindApi.Data;
using NorthwindApi.Models;

namespace NorthwindApi.Tests;

// A fake products table, kept in a list, so the tests need no database.
// Asking for product 666 throws, to test what the API does with an unexpected error.
public class FakeProductDao : IProductDao
{
    public List<Product> Products { get; } = new List<Product>
    {
        new Product { ProductId = 1, ProductName = "Chai", CategoryId = 1, UnitPrice = 18.00m, UnitsInStock = 39 },
        new Product { ProductId = 2, ProductName = "Chang", CategoryId = 1, UnitPrice = 19.00m, UnitsInStock = 17 },
        new Product { ProductId = 3, ProductName = "Aniseed Syrup", CategoryId = 2, UnitPrice = 10.00m, UnitsInStock = 13 }
    };

    public Task<List<Product>> GetAllAsync(int? categoryId)
    {
        List<Product> products = Products
            .Where(p => categoryId == null || p.CategoryId == categoryId)
            .OrderBy(p => p.ProductName)
            .ToList();
        return Task.FromResult(products);
    }

    public Task<Product?> GetByIdAsync(int productId)
    {
        if (productId == 666)
        {
            throw new InvalidOperationException("The fake database broke.");
        }
        return Task.FromResult(Products.FirstOrDefault(p => p.ProductId == productId));
    }

    public Task<Product> AddAsync(Product product)
    {
        product.ProductId = Products.Max(p => p.ProductId) + 1;
        Products.Add(product);
        return Task.FromResult(product);
    }

    public Task<bool> UpdateAsync(int productId, Product product)
    {
        int index = Products.FindIndex(p => p.ProductId == productId);
        if (index < 0)
        {
            return Task.FromResult(false);
        }
        product.ProductId = productId;
        Products[index] = product;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int productId)
    {
        return Task.FromResult(Products.RemoveAll(p => p.ProductId == productId) == 1);
    }
}
