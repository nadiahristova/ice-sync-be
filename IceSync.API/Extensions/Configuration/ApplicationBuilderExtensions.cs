using IceSync.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;

namespace IceSync.API.Extensions.Configuration;

public static class ApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder appBuilder)
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(appBuilder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();
        appBuilder.Logging.ClearProviders();
        appBuilder.Logging.AddSerilog(logger);

        return appBuilder;
    }

    public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error;

                var rfc7807Exception = Rfc7807.Factory(exception, context.Request.Path);

                var logger = app.ApplicationServices.GetService<ILogger<HttpContext>>();
                logger?.LogError(exception, rfc7807Exception.Title);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = rfc7807Exception.Status;

                var result = JsonConvert.SerializeObject(rfc7807Exception);

                await context.Response.WriteAsync(result).ConfigureAwait(false);
            });
        });
    }

    public static void UpdateDatabase<T>(this IApplicationBuilder app)
           where T : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        scope.ServiceProvider.GetRequiredService<T>().Database.Migrate();
    }
}
