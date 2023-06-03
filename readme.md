# My solutions

## TODO #0
To fix this problem endpoint routing was turned off:
```C#
services.AddControllersWithViews(_ => _.EnableEndpointRouting = false);
```

Added creation of `AccountService` service:
```C#
services.AddSingleton<IAccountService, AccountService>();
```

## TODO #1
To add cookie authorization next changes were made in `ConfigureServices` and `ConfigureServices` methods in `Startup`:
1. In method `Configure` in `Startup` were turned on authentication and authorization:
```C#
app.UseAuthentication();
app.UseAuthorization();
```

2. Cookie authentication was turned on in method `ConfigureServices` in `Startup` :
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
1. Changed `Login` method return type in `LoginController` and added [FromRoute] (but I don't think that's right... userName is like auth information and in real usage we are going to perform validation by user data that in this case is userName):
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

As you can see, this method returns shallow copy, so all primitive types changes happen with this one and not the original object. As far as I can see, this issue can be fixed with either of two ways:
1. Return the original object (remove .Clone() call)
2. Leave .Clone() and update the database and cache and change values there

If we pay attention to the method's name `GetOrCreate`, than we'll see that it combines idempotent and not idempotent operations. If we don't create any more functions when I don't see the problem with sticking to the first approach and adding another not idempotent opration by removing .Clone() call. 

If we don't want to remove .Clone(), than I think it's better to add another function that will change db state.

So, the first approach is to change this:
```C#
return Task.FromResult(account.Clone());
```

on:
```C#
return Task.FromResult(account);
```

And the second one:
1. Change `UpdateAccount` in `AccountController.cs`
```C#
//Update account in cache, don't bother saving to DB, this is not an objective of this task.
var account = await Get();
_accountService.UpdateCounter(account.ExternalId);
```

2. Create update functions in `AccountService`:
```C#
public async void UpdateCounter(string id)
{
  _db.UpdateCounter(id);
  var account = await _db.GetOrCreateAccountAsync(id);
  _cache.AddOrUpdate(account);
}
        
 public async void UpdateCounter(long id)
 {
  _db.UpdateCounter(id);
  var account = await _db.GetOrCreateAccountAsync(id);
  _cache.AddOrUpdate(account);
 }
```

3. And create update functions in `AccountDatabaseStub`:
```C#
public void UpdateCounter(string id)
{
  lock (this)
  {
    var account = _accounts.FirstOrDefault(x => x.Value.ExternalId == id).Value;
      if (account != null)
      {
        _accounts[account.ExternalId].Counter++;
      }
  }
}

public void UpdateCounter(long id)
{
  lock (this)
  {
    var account = _accounts.FirstOrDefault(x => x.Value.InternalId == id).Value;
      if (account != null)
      {
        _accounts[account.ExternalId].Counter++;
      }
  }
}
```

4. Create function signatures in `IAccountService`, `IAccountDatabase`
