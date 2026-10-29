namespace NorthwindApi.Models;

// A user, what a visitor sends to sign in, and what the API sends back. They're finished.

public class AppUser
{
    public int UserId { get; set; }
    public string UserName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "User";
}

public record Credentials(string UserName, string Password);

public record AuthResult(string Token, string UserName, string Role);
