using AspNetCoreHero.ToastNotification;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PojisteniApp2.Data;

namespace PojisteniApp2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Changed based on tutorial https://www.itnetwork.cz/csharp/asp-net-core/zaklady/registrace-v-aspnet-core-mvc
            //builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            // Toast notifications - explained at https://codewithmukesh.com/blog/toast-notifications-in-aspnet-core/
            builder.Services.AddNotyf(config => { config.DurationInSeconds = 5; config.IsDismissable = true; config.Position = NotyfPosition.TopCenter; });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Commented out based on tutorial https://www.itnetwork.cz/csharp/asp-net-core/zaklady/registrace-v-aspnet-core-mvc
            //app.MapRazorPages();

            // Based on tutorial https://www.itnetwork.cz/csharp/asp-net-core/zaklady/uzivatelske-role-v-aspnet-core-mvc-a-dokonceni-blogu
            //using (var scope = app.Services.CreateScope())
            //{
            //    RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            //    UserManager<IdentityUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            //    roleManager.CreateAsync(new IdentityRole("admin")).Wait();
            //    IdentityUser user = userManager.FindByEmailAsync("admin@inshuro.com").Result;
            //    userManager.AddToRoleAsync(user, "admin").Wait();
            //}

            app.Run();
        }
    }
}