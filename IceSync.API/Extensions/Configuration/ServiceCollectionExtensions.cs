using AspNetCoreRateLimit;
using IceSync.Domain.Dtos;
using IceSync.Domain.Interfaces;
using IceSync.Domain.Interfaces.HttpClients;
using IceSync.Domain.Interfaces.Repositories;
using IceSync.Domain.Settings;
using IceSync.Infrastructure;
using IceSync.Infrastructure.BackgroundServices;
using IceSync.Infrastructure.Mediator.Handlers;
using IceSync.Infrastructure.Mediator.Requests;
using IceSync.Infrastructure.Policies;
using IceSync.Infrastructure.Repositories;
using IceSync.Infrastructure.Services;
using IceSync.Infrastructure.Services.TokenManagement;
using IdempotentAPI.Cache.DistributedCache.Extensions.DependencyInjection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Refit;

namespace IceSync.API.Extensions.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSettings(this IServiceCollection services, ConfigurationManager configurationManager)
    {
        services.Configure<UniversalLoaderSyncSettings>(configurationManager.GetSection(nameof(UniversalLoaderSyncSettings)));
        services.Configure<PollyPolicySettings>(configurationManager.GetSection(nameof(PollyPolicySettings)));
        
        return services;
    }

    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<UniversalLoaderSyncBackgroundService>();

        return services;
    }

    public static IServiceCollection AddRefitClients(this IServiceCollection services, ConfigurationManager configurationManager)
    {
        var configSection = configurationManager.GetSection(nameof(UniversalLoaderSettings));
        var universalLoaderSettings = configSection.Get<UniversalLoaderSettings>();

        services.AddRefitClient<IUniversalLoaderHttpClient>()
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(universalLoaderSettings.Url);
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.Accept, "application/json");
            });

        services.Configure<UniversalLoaderSettings>(configSection);

        return services;
    }

    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<ITokenService, UniversalLoaderTokenService>();
        services.AddScoped<IUniversalLoaderService, UniversalLoaderService>();
        services.AddTransient<IUniversalLoaderSyncService, UniversalLoaderSyncService>();

        services.AddScoped<IWorkflowReporitory, WorkflowReporitory>();
        
        services.AddSingleton<IRefitPolicyManager, RefitPolicyManager>();

        return services;
    }

    public static IServiceCollection RegisterMediatR(this IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<RunWorkflowRequest, Unit>, RunWorkflowHandler>();
        services.AddTransient<IRequestHandler<GetAllWorkflowsRequest, IEnumerable<WorkflowDto>>, GetAllWorkflowsHandler>();
        services.AddTransient<IRequestHandler<GetWorkflowExecutionsRequest, IEnumerable<WorkflowExecutionDto>>, GetWorkflowExecutionsHandler>();

        return services;
    }

    public static IServiceCollection AddContext(this IServiceCollection services, ConfigurationManager configurationManager)
    {
        var connctionString = configurationManager.GetConnectionString("SqlServer");

        services.AddDbContext<IceSyncContext>(options =>
                options.UseSqlServer(connctionString, options => options.MigrationsAssembly(typeof(IceSyncContext).Assembly.GetName().Name)), 
                ServiceLifetime.Scoped, ServiceLifetime.Scoped);

        return services;
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        return services;
    }

    public static IServiceCollection AddIdempotencySupport(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddIdempotentAPIUsingDistributedCache();

        return services;
    }
}
