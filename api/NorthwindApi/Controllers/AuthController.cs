using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NorthwindApi.Data;
using NorthwindApi.Models;
using NorthwindApi.Security;

namespace NorthwindApi.Controllers;

// Registering and signing in.
//
// Requirements:
// - POST /api/auth/register with { userName, password } adds a user with the role "User", storing only the
//   password's hash from the PasswordHasher, and answers 200 with an AuthResult: a token, the user name
//   and the role.
//   - A blank user name, or a password shorter than 8 characters: 400 Bad Request, with the text
//     "Choose a user name, and a password of at least 8 characters."
//   - A user name that's taken: 409 Conflict, with the text "The user name {userName} is taken."
// - POST /api/auth/login with { userName, password } answers 200 with an AuthResult when the password is
//   right, and 401 Unauthorized, with the text "That user name and password don't match.", when the user
//   doesn't exist or the password is wrong.
// - GET /api/auth/me needs a token, and answers 200 with { userName, role } from the token.
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserDao userDao;
    private readonly TokenService tokenService;
    private readonly PasswordHasher<AppUser> hasher = new PasswordHasher<AppUser>();

    public AuthController(IUserDao userDao, TokenService tokenService)
    {
        this.userDao = userDao;
        this.tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register(Credentials credentials)
    {
        // TODO: check the input, refuse a taken name, add the user, and sign them in.
        await Task.CompletedTask;
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login(Credentials credentials)
    {
        // TODO: check the password, and sign the user in.
        await Task.CompletedTask;
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    // TODO: GET /api/auth/me
}
