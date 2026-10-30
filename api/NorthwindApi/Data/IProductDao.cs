using NorthwindApi.Models;

namespace NorthwindApi.Data;

// What the API needs from the products table. It's finished.
public interface IProductDao
{
    // Every product in order of name; only one category's when categoryId isn't null.
    Task<List<Product>> GetAllAsync(int? categoryId);

    // The product with that id, or null.
    Task<Product?> GetByIdAsync(int productId);

    // Saves a new product, and returns it with the id the database gave it.
    Task<Product> AddAsync(Product product);

    // Replaces a product's values; false if there's no product with that id.
    Task<bool> UpdateAsync(int productId, Product product);

    // Deletes a product; false if there's no product with that id.
    Task<bool> DeleteAsync(int productId);
}
