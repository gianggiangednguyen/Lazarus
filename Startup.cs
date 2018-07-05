using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Lazarus.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lazarus
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSession();
            services.AddDistributedMemoryCache();

            services.AddDbContext<LazarusDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.ConfigureApplicationCookie(options =>
                {
                    options.Cookie.Expiration = TimeSpan.FromMinutes(60);
                });

            services.Configure<CookiePolicyOptions>(options =>
                options.MinimumSameSitePolicy = SameSiteMode.None);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            //Claim-based policy
            services.AddAuthorization(options =>
                {
                    options.AddPolicy("ManagerPolicy", policy => policy.RequireClaim(ClaimTypes.Role, new string[] { "Admin", "ShopManager" }));
                    options.AddPolicy("AdminPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
                    options.AddPolicy("ShopManagerPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "ShopManager"));
                    options.AddPolicy("NormalUserPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "NormalUser"));
                    options.AddPolicy("DeliverPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "Deliver"));
                    options.AddPolicy("AllUserPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "Admin", "ShopManager", "NormalUser"));
                    options.AddPolicy("UnverifiedUserPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "UnverifiedUser"));
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSession();

            app.UseAuthentication();

            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
