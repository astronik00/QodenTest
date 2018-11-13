using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp
{
    public class Startup
    {
        // TODO 0: Something broken in ConfigureServices.
        // Don't focus attention on it right now, you will faced with problem in process of meeting the challenges TODOs
        // Pay attention to different possible solutions of the problem

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(_ => _.EnableEndpointRouting = false);
            services.AddSingleton<IAccountService, AccountService>();
            services.AddSingleton<IAccountDatabase, AccountDatabaseStub>();
            services.AddSingleton<IAccountCache, AccountCache>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.Events.OnRedirectToLogin = (context) => {
                            context.Response.StatusCode = 401;
                            return Task.CompletedTask;
                        };
                    });
            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseStaticFiles();
            app.UseRouting();
            app.UseMvc();
            
            app.Run(async (context) => { await context.Response.WriteAsync("Hello World!"); });
        }
    }
}