    using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApp.Identity
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
            services.AddControllersWithViews();
            var connectionString = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=IdentityCurso;Data Source=DESKTOP-ROLELGP\SQLEXPRESS";
            var migrationAssembly = typeof(Startup)
                .GetTypeInfo().Assembly
                .GetName().Name;
            services.AddDbContext<MyUserDbContext>(opt => opt.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationAssembly)));
            //services.AddIdentityCore<MyUser>(opt => { });
            //services.AddScoped<IUserStore<MyUser>, MyUserStore>();
            //services.AddIdentityCore<MyUser>(opt => { });

            services.AddIdentity<MyUser, IdentityRole>(opt => {
                opt.SignIn.RequireConfirmedEmail = true;
                opt.Password.RequireDigit = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequiredLength = 4;

                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.Lockout.AllowedForNewUsers = true;
            })
                .AddEntityFrameworkStores<MyUserDbContext>()
                .AddDefaultTokenProviders()
                .AddPasswordValidator<DoesNotContainsPasswordValidator<MyUser>>();

            services.Configure<DataProtectionTokenProviderOptions>(
                opt => opt.TokenLifespan = TimeSpan.FromHours(3)
                );

            services.AddScoped<IUserStore<MyUser>, UserOnlyStore<MyUser, MyUserDbContext>>()                ;
            services.AddScoped<IUserClaimsPrincipalFactory<MyUser>, MyUserClaimsPrincipalFactory>();
            
            services.ConfigureApplicationCookie( opt => opt.LoginPath = "/Home/Login");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
