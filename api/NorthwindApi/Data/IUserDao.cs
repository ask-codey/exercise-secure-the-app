using NorthwindApi.Models;

namespace NorthwindApi.Data;

// Reads and adds users. It's finished.
public interface IUserDao
{
    Task<AppUser?> GetByUserNameAsync(string userName);
    Task AddAsync(AppUser user);
}
