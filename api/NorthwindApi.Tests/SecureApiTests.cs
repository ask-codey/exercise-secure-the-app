using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NorthwindApi.Data;
using NorthwindApi.Models;

namespace NorthwindApi.Tests;

// These tests start your whole API in memory and send it requests, the way a client would.
// They swap the real DAOs for fakes, so they need no database, and give the API a test signing key,
// so they need no user secrets. You don't need to change this file.
[TestClass]
public class SecureApiTests
{
    private const string TestKey = "a-test-signing-key-that-is-long-enough!";

    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;
    private FakeProductDao products = null!;
    private FakeUserDao users = null!;

    [TestInitialize]
    public void StartTheApi()
    {
        products = new FakeProductDao();
        users = new FakeUserDao();
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Jwt:Key", TestKey);
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IProductDao>(_ => products);
                services.AddScoped<IUserDao>(_ => users);
            });
        });
        client = factory.CreateClient();
    }

    [TestCleanup]
    public void StopTheApi()
    {
        client.Dispose();
        factory.Dispose();
    }

    private static Product NewProduct(string name) =>
        new Product { ProductName = name, CategoryId = 3, UnitPrice = 4.50m, UnitsInStock = 20 };

    private async Task<HttpResponseMessage> Send(HttpMethod method, string url, string? token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        if (body != null)
        {
            request.Content = JsonContent.Create(body);
        }
        return await client.SendAsync(request);
    }

    private async Task<string> SignIn(string userName, string password)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", new Credentials(userName, password));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"Signing in as {userName} should work.");
        AuthResult? result = await response.Content.ReadFromJsonAsync<AuthResult>();
        return result!.Token;
    }

    private async Task<string> RegisterAda()
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", new Credentials("ada", "summer2026"));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Registering ada should work.");
        AuthResult? result = await response.Content.ReadFromJsonAsync<AuthResult>();
        return result!.Token;
    }

    [TestMethod]
    public async Task Register_NewUser_StoresAHashAndAnswersAToken()
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", new Credentials("ada", "summer2026"));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        AuthResult? result = await response.Content.ReadFromJsonAsync<AuthResult>();
        Assert.AreEqual("ada", result!.UserName);
        Assert.AreEqual("User", result.Role);
        Assert.HasCount(3, result.Token.Split('.'), "A JSON Web Token has three parts.");

        AppUser? stored = users.Users.FirstOrDefault(u => u.UserName == "ada");
        Assert.IsNotNull(stored, "The user should be added with the IUserDao.");
        Assert.AreNotEqual("summer2026", stored.PasswordHash, "Store the password's hash, never the password.");
        Assert.AreEqual(PasswordVerificationResult.Success,
            new PasswordHasher<AppUser>().VerifyHashedPassword(stored, stored.PasswordHash, "summer2026"),
            "The hash should come from PasswordHasher.");
    }

    [TestMethod]
    public async Task Register_TakenUserName_Answers409()
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", new Credentials("grace", "another-password"));

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        Assert.AreEqual("The user name grace is taken.", await response.Content.ReadAsStringAsync());
        Assert.HasCount(1, users.Users);
    }

    [TestMethod]
    public async Task Register_ShortPasswordOrBlankName_Answers400()
    {
        HttpResponseMessage shortPassword = await client.PostAsJsonAsync("/api/auth/register", new Credentials("ada", "short"));
        HttpResponseMessage blankName = await client.PostAsJsonAsync("/api/auth/register", new Credentials(" ", "summer2026"));

        Assert.AreEqual(HttpStatusCode.BadRequest, shortPassword.StatusCode);
        Assert.AreEqual("Choose a user name, and a password of at least 8 characters.", await shortPassword.Content.ReadAsStringAsync());
        Assert.AreEqual(HttpStatusCode.BadRequest, blankName.StatusCode);
        Assert.HasCount(1, users.Users);
    }

    [TestMethod]
    public async Task Login_WrongPasswordOrUnknownUser_Answers401WithTheSameMessage()
    {
        HttpResponseMessage wrongPassword = await client.PostAsJsonAsync("/api/auth/login", new Credentials("grace", "wrong-password"));
        HttpResponseMessage unknownUser = await client.PostAsJsonAsync("/api/auth/login", new Credentials("nobody", "grace-password"));

        Assert.AreEqual(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.AreEqual("That user name and password don't match.", await wrongPassword.Content.ReadAsStringAsync());
        Assert.AreEqual(HttpStatusCode.Unauthorized, unknownUser.StatusCode);
        Assert.AreEqual("That user name and password don't match.", await unknownUser.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task Login_RightPassword_AnswersASignedTokenWithTheClaims()
    {
        string token = await SignIn("grace", "grace-password");

        var parameters = new TokenValidationParameters
        {
            ValidIssuer = "northwind-api",
            ValidAudience = "northwind-app",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey))
        };
        TokenValidationResult result = await new JsonWebTokenHandler().ValidateTokenAsync(token, parameters);

        Assert.IsTrue(result.IsValid, "The token should be signed with Jwt:Key, with the Jwt issuer and audience.");
        Assert.AreEqual("grace", result.Claims["sub"]);
        Assert.AreEqual("Admin", result.Claims["role"]);
        DateTime expires = ((JsonWebToken)result.SecurityToken).ValidTo;
        Assert.IsTrue(expires > DateTime.UtcNow.AddMinutes(55) && expires < DateTime.UtcNow.AddMinutes(65),
            "The token should expire after Jwt:Minutes (60) minutes.");
    }

    [TestMethod]
    public async Task Me_WithAndWithoutAToken()
    {
        string token = await SignIn("grace", "grace-password");

        HttpResponseMessage signedIn = await Send(HttpMethod.Get, "/api/auth/me", token);
        HttpResponseMessage anonymous = await Send(HttpMethod.Get, "/api/auth/me", null);

        Assert.AreEqual(HttpStatusCode.OK, signedIn.StatusCode);
        Dictionary<string, string>? me = await signedIn.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.AreEqual("grace", me!["userName"]);
        Assert.AreEqual("Admin", me["role"]);
        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);
    }

    [TestMethod]
    public async Task Add_WithoutAToken_Answers401_WhileGetStaysOpen()
    {
        HttpResponseMessage add = await Send(HttpMethod.Post, "/api/products", null, NewProduct("Trail Mix"));
        HttpResponseMessage get = await Send(HttpMethod.Get, "/api/products", null);

        Assert.AreEqual(HttpStatusCode.Unauthorized, add.StatusCode);
        Assert.HasCount(3, products.Products, "Nothing should be added without a token.");
        Assert.AreEqual(HttpStatusCode.OK, get.StatusCode, "Anyone may read the products.");
    }

    [TestMethod]
    public async Task Add_WithAToken_Answers201()
    {
        string token = await RegisterAda();

        HttpResponseMessage response = await Send(HttpMethod.Post, "/api/products", token, NewProduct("Trail Mix"));

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.HasCount(4, products.Products);
    }

    [TestMethod]
    public async Task Update_NeedsAToken()
    {
        string token = await RegisterAda();

        HttpResponseMessage anonymous = await Send(HttpMethod.Put, "/api/products/1", null, NewProduct("Chai Latte"));
        HttpResponseMessage signedIn = await Send(HttpMethod.Put, "/api/products/1", token, NewProduct("Chai Latte"));

        Assert.AreEqual(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.AreEqual(HttpStatusCode.NoContent, signedIn.StatusCode);
        Assert.AreEqual("Chai Latte", products.Products.First(p => p.ProductId == 1).ProductName);
    }

    [TestMethod]
    public async Task Delete_AsAUserWhoIsNotAnAdmin_Answers403()
    {
        string token = await RegisterAda();

        HttpResponseMessage response = await Send(HttpMethod.Delete, "/api/products/2", token);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.HasCount(3, products.Products);
    }

    [TestMethod]
    public async Task Delete_AsAnAdmin_Answers204()
    {
        string token = await SignIn("grace", "grace-password");

        HttpResponseMessage response = await Send(HttpMethod.Delete, "/api/products/2", token);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        Assert.HasCount(2, products.Products);
    }

    [TestMethod]
    public async Task Add_WithATokenSignedWithAnotherKey_Answers401()
    {
        var otherKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("somebody-else's-key-that-is-long-enough"));
        string forged = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("sub", "grace"), new Claim("role", "Admin")]),
            Issuer = "northwind-api",
            Audience = "northwind-app",
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = new SigningCredentials(otherKey, SecurityAlgorithms.HmacSha256)
        });

        HttpResponseMessage response = await Send(HttpMethod.Delete, "/api/products/2", forged);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.HasCount(3, products.Products);
    }
}
