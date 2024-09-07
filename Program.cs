using Experiments.Services;
using Microsoft.EntityFrameworkCore;

namespace Experiments
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<SignUpContext>(options => {
                options.UseSqlServer(connectionString);
            });
            builder.Services.AddDbContext<LoginContext>(options => {
                options.UseSqlServer(connectionString);
            });

            var app = builder.Build();
            

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
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
                name: "Home",
                pattern: "{controller=Home}/{action=HomePage}");

            app.MapControllerRoute(
                name: "Login",
                pattern: "{controller=Login}/{action=Login}");

            app.MapControllerRoute(
                name: "About",
                pattern: "{controller=About}/{action=About}");


            app.MapControllerRoute(
                name: "SignUp",
                pattern: "{controller=SignUp}/{action=SignUp}");



            app.Run();
        }
    }
}
