using BloggoApi.Contexts;
using BloggoApi.Services;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Json;

namespace BloggoApi;

public class Program
{
    // TODO: figure out a *good* way to properly deliniate api version by a /api/{version}/... convention
    // maybe: https://www.nuget.org/packages/Asp.Versioning.Mvc
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.Async(a =>
                a.File(
                    new JsonFormatter(renderMessage: true),
                    "logs/bloggo_api_.log",
                    rollingInterval: RollingInterval.Day
                )
            )
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        // Add services to the container.
        builder.Services.AddSerilog();
        builder.Services.AddDbContext<SqliteDbContext>();
        builder.Services.AddControllersWithViews();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Bloggo API",
                    Description = "Blog posting MVC backend api",
                }
            );
        });

        builder.Services.AddSingleton<DeletionService>();
        builder.Services.AddRouting(options => options.LowercaseUrls = true);
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            Log.Information("develop environment, enabling swagger and ui at /swagger");
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = "swagger";
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseStaticFiles("/wwwroot");

        app.MapControllers();

        try
        {
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "backend API terminated from exception");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
