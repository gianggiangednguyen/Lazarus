using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lazarus.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
                options.Cookie.Expiration = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
            });

            services.AddAuthentication("LazarusSchema")
                .AddCookie("LazarusSchema", options =>
                {
                    options.AccessDeniedPath = "";
                    options.Cookie = new CookieBuilder
                    {
                        HttpOnly = true,
                        Name = "Lazarus.Security.Cookie",
                        Path = "/"
                    };
                }
                );
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
