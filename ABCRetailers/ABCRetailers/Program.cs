using ABCRetailers.Services;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace ABCRetailers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC
            builder.Services.AddControllersWithViews();

            builder.Services.Configure<FunctionsOptions>(
                builder.Configuration.GetSection("Functions"));

            builder.Services.AddHttpClient<IFunctionsApi, FunctionsApiClient>();

            // Logging
            builder.Services.AddLogging();

            var app = builder.Build();

            // Culture
            var culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}



