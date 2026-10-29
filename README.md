# Practice: Secure the App

Practice code for the lesson **Practice: Secure the App** in the C# Full-Stack Bootcamp on [ask-codey.com](https://www.ask-codey.com).

The products API and its Angular client, from *Practice: Northwind Web API* and *Practice: an Angular Client for a Web API*, get users. Anyone can read the products, signed-in users can add and change them, and only admins can delete them.

## What's here

| Folder | What it is | Tests |
|---|---|---|
| `api/` | The products API (ASP.NET Core, Dapper) | `dotnet test`, in `api/` |
| `client/` | The Angular client, with a proxy that sends `/api` requests to http://localhost:5179 | `npm install`, then `npm test`, in `client/` |

Both test suites use fakes, so they need no database, no user secrets and no running API.

### In api/

- `Users.sql`: the users table. Run it once in SSMS, in your Northwind database.
- `NorthwindApi/Models/AppUser.cs`, `Data/IUserDao.cs`, `Data/UserDao.cs`: users, and the DAO for them. Ready to use.
- `NorthwindApi/Security/TokenService.cs`: makes the tokens, for you to finish.
- `NorthwindApi/Controllers/AuthController.cs`: register, login and "who am I", for you to finish.
- `NorthwindApi/Program.cs`: turn on JWT bearer authentication here.
- `NorthwindApi/Controllers/ProductsController.cs`: protect its actions.
- `NorthwindApi/NorthwindApi.http`: requests to try it by hand; requests 11 to 16 are for this practice.

### In client/src/app/

- `auth.ts`: the Auth service, for you to finish.
- `login/`: the sign-in page, for you to finish.
- `auth-interceptor.ts` and `signed-in-guard.ts`: for you to finish.
- `app.config.ts` and `app.routes.ts`: switch the interceptor, input binding and the guard on.
- `product-list/product-list.html`: show the Add link and the Delete buttons only to the right people.
- `app.ts`, `new-product/`, `product-api.ts`, `product.ts`: ready to use.

The requirements are in the comment at the top of each file.

## What to do

1. **The API.** Finish `TokenService` and `AuthController`, turn on authentication in `Program.cs`, and add `[Authorize]` to the products actions. Run `dotnet test` in `api/` as you go.
2. **The client.** Finish `Auth`, `Login`, `authInterceptor` and `signedInGuard`, switch them on, and change the products page's template. Run `npm test` in `client/` as you go.

## Try it for real

1. Run `Users.sql` in SSMS.
2. In `api/NorthwindApi`, make a key and store it in user secrets (see *Securing a Web API with JWT Bearer*):

   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:Key" "<your key>"
   ```

3. Start the API (**Ctrl+F5**), and register a user with request 11 in `NorthwindApi.http`. To make them an admin: `UPDATE Users SET Role = 'Admin' WHERE UserName = 'ada';`
4. In `client/`, run `ng serve`, and open http://localhost:4200.

## Tips

- The lessons *Securing a Web API with JWT Bearer*, *Login and Register in Angular* and *Route Guards and HTTP Interceptors* have everything you need.
- A test's message says what it expected and what it found. Read it before changing code.
- `dotnet test` starts your whole API in memory, with a test key: if every API test fails with a 500, look at `Program.cs` first.

## Hand it in

When it works, push your solution to a public repo of your own on GitHub, then paste that repo's address into **Submit your exercise** on the lesson page at [ask-codey.com](https://www.ask-codey.com). That's what completes the lesson. The lesson page shows the Git commands, step by step.
