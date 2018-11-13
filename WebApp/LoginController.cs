using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebApp
{
    [Route("api")]
    public class LoginController : Controller
    {
        private readonly IAccountDatabase _db;

        public LoginController(IAccountDatabase db)
        {
            _db = db;
        }

        [HttpPost("sign-in/{userName}")]
        public async Task<IActionResult> Login([FromRoute] string userName)
        {
            var account = await _db.FindByUserNameAsync(userName);

            if (account != null)
            {
                var id = account.ExternalId;
                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, account.ExternalId),
                    new Claim(ClaimTypes.Name, account.UserName),
                    new Claim(ClaimTypes.Role, account.Role)
                }; 
                
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); 
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                
                return Ok(account);
                
                //TODO 1: Generate auth cookie for user 'userName' with external id
                //Done
            }
            //TODO 2: return 404 if user not found
            //Done
            return NotFound();
        }
    }
}