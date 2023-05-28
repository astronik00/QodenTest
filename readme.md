# My solutions

## TODO #0
To fix this problem was turned off endpoint routing:
```C#
services.AddControllersWithViews(_ => _.EnableEndpointRouting = false);
```

And added creating of `AccountService` service:
```C#
services.AddSingleton<IAccountService, AccountService>();
```

## TODO #1
To add cookie authorization next changes were made in `ConfigureServices` and `ConfigureServices` methods in `Startup`:
1. In method `Configure` in `Startup` turned on authentication and authorization:
```C#
app.UseAuthentication();
app.UseAuthorization();
```

2. Turned on cookies authentication in method `ConfigureServices` in `Startup` :
```C#
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                });
        services.AddAuthorization();
```

3. Changed `Login` method return type in `LoginController`:
```C#
public async Task<IActionResult> Login([FromRoute] string userName)
```

4. Added next lines in method `Login` in `LoginController` to create cookie for existing account:
```C#
if (account != null)
        {
            var id = account.ExternalId;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.ExternalId),
                new(ClaimTypes.Name, account.UserName),
                new(ClaimTypes.Role, account.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return Ok(account);
        }
```

## TODO #2
1. Changed `Login` method return type in `LoginController`:
```C#
public async Task<IActionResult> Login([FromRoute] string userName)
```
2. Added next line in method `Login` to return 404 Exception:
```C#
return NotFound();
```

## TODO #3
Added next line in method `Get` in AccountController to get userId from cookie:
```C#
var userId = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
```

## TODO #4
1. Added annotation [Authorize] on whole controller:
```C#
[Authorize]
[Route("api/account")]
public class AccountController : Controller
```

2. Added next code in `ConfigureServices` function, so that 401 will always return if user isn't authorized and trying to reach protected resources:
```C#
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                });
```

## TODO #5
Added next annotation on method `GetByInternalId` in `AccountController.cs`
```C#
[Authorize(Roles = "Admin")]
```

## TODO #6
The described dysfunctionality comes from method `GetOrCreateAccountAsync` in `AccountDatabaseStub.cs`. The problem is in the next line:
```C#
return Task.FromResult(account.Clone());
```

As you can see, this method returns copy, so all changes happen with this one and not the original object. As far as I can see, this issue can be fixed with either of two ways:
1. Return the original object (remove Clone() call)
2. Leave .Clone() and update the database and cache and change values there

If we pay attention to the method's name `GetOrCreate`, than we'll see that it combines idempotent and not idempotent operations. If we don't create any more functions when I don't see the problem with sticking to the first approach and adding another not idempotent opration by removing .Clone() call. If we don't want to remove .Clone(), than we'll facing the need to create another function, but if we create another function, why don't split idempotent and not idempotent operations?


