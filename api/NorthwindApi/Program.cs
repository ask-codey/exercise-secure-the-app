using NorthwindApi.Data;
using NorthwindApi.Errors;
using NorthwindApi.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DatabaseConflictHandler>();

string connectionString = builder.Configuration.GetConnectionString("Northwind")!;
builder.Services.AddScoped<IProductDao>(_ => new ProductDao(connectionString));
builder.Services.AddScoped<IUserDao>(_ => new UserDao(connectionString));
builder.Services.AddSingleton<TokenService>();

// TODO: turn on JWT bearer authentication, checking the issuer, the audience and the key from the
// Jwt settings, with "sub" as the name claim and "role" as the role claim.

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// TODO: authentication goes here, before authorization.
app.UseAuthorization();

app.MapControllers();

app.Run();

// Lets the tests start the app.
public partial class Program;
