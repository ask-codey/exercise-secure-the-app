using Microsoft.AspNetCore.Identity;
using NorthwindApi.Data;
using NorthwindApi.Models;

namespace NorthwindApi.Tests;

// A fake users table, kept in a list, so the tests need no database.
// It starts with one admin, grace, whose password is "grace-password".
public class FakeUserDao : IUserDao
{
    public List<AppUser> Users { get; } = new List<AppUser>();

    public FakeUserDao()
    {
        var grace = new AppUser { UserId = 1, UserName = "grace", Role = "Admin" };
        grace.PasswordHash = new PasswordHasher<AppUser>().HashPassword(grace, "grace-password");
        Users.Add(grace);
    }

    public Task<AppUser?> GetByUserNameAsync(string userName)
    {
        return Task.FromResult(Users.FirstOrDefault(u => u.UserName == userName));
    }

    public Task AddAsync(AppUser user)
    {
        user.UserId = Users.Max(u => u.UserId) + 1;
        Users.Add(user);
        return Task.CompletedTask;
    }
}
