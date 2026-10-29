using Dapper;
using Microsoft.Data.SqlClient;
using NorthwindApi.Models;

namespace NorthwindApi.Data;

// The products table, with Dapper. It's finished.
public class ProductDao : IProductDao
{
    private const string Columns = "ProductId, ProductName, CategoryId, UnitPrice, UnitsInStock, Discontinued";
    private readonly string connectionString;

    public ProductDao(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task<List<Product>> GetAllAsync(int? categoryId)
    {
        string sql = $"""
            SELECT {Columns}
            FROM Products
            WHERE @CategoryId IS NULL OR CategoryId = @CategoryId
            ORDER BY ProductName;
            """;

        using SqlConnection connection = new SqlConnection(connectionString);
        IEnumerable<Product> products = await connection.QueryAsync<Product>(sql, new { CategoryId = categoryId });
        return products.ToList();
    }

    public async Task<Product?> GetByIdAsync(int productId)
    {
        using SqlConnection connection = new SqlConnection(connectionString);
        return await connection.QuerySingleOrDefaultAsync<Product>(
            $"SELECT {Columns} FROM Products WHERE ProductId = @ProductId;", new { ProductId = productId });
    }

    public async Task<Product> AddAsync(Product product)
    {
        string sql = """
            INSERT INTO Products (ProductName, CategoryId, UnitPrice, UnitsInStock, Discontinued)
            OUTPUT INSERTED.ProductId
            VALUES (@ProductName, @CategoryId, @UnitPrice, @UnitsInStock, @Discontinued);
            """;

        using SqlConnection connection = new SqlConnection(connectionString);
        product.ProductId = await connection.ExecuteScalarAsync<int>(sql, product);
        return product;
    }

    public async Task<bool> UpdateAsync(int productId, Product product)
    {
        string sql = """
            UPDATE Products
            SET ProductName = @ProductName, CategoryId = @CategoryId, UnitPrice = @UnitPrice,
                UnitsInStock = @UnitsInStock, Discontinued = @Discontinued
            WHERE ProductId = @ProductId;
            """;

        using SqlConnection connection = new SqlConnection(connectionString);
        int rows = await connection.ExecuteAsync(sql, new
        {
            ProductId = productId,
            product.ProductName,
            product.CategoryId,
            product.UnitPrice,
            product.UnitsInStock,
            product.Discontinued
        });
        return rows == 1;
    }

    public async Task<bool> DeleteAsync(int productId)
    {
        using SqlConnection connection = new SqlConnection(connectionString);
        int rows = await connection.ExecuteAsync("DELETE FROM Products WHERE ProductId = @ProductId;", new { ProductId = productId });
        return rows == 1;
    }
}
