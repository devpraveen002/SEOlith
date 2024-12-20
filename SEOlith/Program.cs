using Serilog;
using SEOlith.Services;
using SEOlith.Contexts;
using SEOlith.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;

namespace SEOlith
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load environment-specific configuration
            builder.Configuration
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // Configure Serilog based on environment
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName();

            if (builder.Environment.IsDevelopment())
            {
                logConfig
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File("logs/seoaudit_dev_.log",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
            }
            else
            {
                logConfig
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File("logs/seoaudit_prod_.log",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
            }

            Log.Logger = logConfig.CreateLogger();
            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient();

            // Configure database based on environment
            builder.Services.AddDbContext<SeolithDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(3);
                        npgsqlOptions.CommandTimeout(30);
                    });

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            builder.Services.AddScoped<ISeoAuditService, SeoAuditService>();
            builder.Services.AddScoped<GlobalExceptionHandler>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
                Log.Information("Running in Development environment");
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                Log.Information("Running in Production environment");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseMiddleware<GlobalExceptionHandler>();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Apply migrations based on environment
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<SeolithDbContext>();
                    if (app.Environment.IsDevelopment())
                    {
                        Log.Information("Applying database migrations in Development");
                        context.Database.Migrate();
                    }
                    else
                    {
                        // In production, verify connection without automatic migration
                        Log.Information("Verifying database connection in Production");
                        context.Database.CanConnect();
                    }
                }

                Log.Information("Starting application");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}