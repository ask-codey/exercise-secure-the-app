using NorthwindApi.Models;

namespace NorthwindApi.Security;

// Makes the JSON Web Token that the API hands out at sign-in.
//
// Requirements:
// - CreateToken(user) returns a token signed with HMAC-SHA256 and the key in the setting Jwt:Key.
// - The token's claims are "sub", the user name, and "role", the user's role.
// - Its issuer and audience are the settings Jwt:Issuer and Jwt:Audience, and it expires after
//   Jwt:Minutes minutes.
public class TokenService
{
    private readonly IConfiguration configuration;

    public TokenService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public string CreateToken(AppUser user)
    {
        // TODO: make and return the token.
        return "";
    }
}
