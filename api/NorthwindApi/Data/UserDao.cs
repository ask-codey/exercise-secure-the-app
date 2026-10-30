using Dapper;
using Microsoft.Data.SqlClient;
using NorthwindApi.Models;

namespace NorthwindApi.Data;

// The users table, with Dapper. It's finished.
public class UserDao : IUserDao
{
    private readonly string connectionString;

    public UserDao(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task<AppUser?> GetByUserNameAsync(string userName)
    {
        using SqlConnection connection = new SqlConnection(connectionString);
        return await connection.QuerySingleOrDefaultAsync<AppUser>(
            "SELECT UserId, UserName, PasswordHash, Role FROM Users WHERE UserName = @UserName;",
            new { UserName = userName });
    }

    public async Task AddAsync(AppUser user)
    {
        string sql = """
            INSERT INTO Users (UserName, PasswordHash, Role)
            OUTPUT INSERTED.UserId
            VALUES (@UserName, @PasswordHash, @Role);
            """;

        using SqlConnection connection = new SqlConnection(connectionString);
        user.UserId = await connection.ExecuteScalarAsync<int>(sql, user);
    }
}
