using AspNetCoreRateLimit;
using FluentValidation.AspNetCore;
using IceSync.API.Extensions.Configuration;
using IceSync.Infrastructure;
using MediatR;
using System.Reflection;

const string CorsPolicyName = "pub";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddFluentValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddContext(builder.Configuration);

builder.Services.AddSettings(builder.Configuration);

builder.Services.RegisterServices();
builder.Services.RegisterMediatR();

builder.Services.AddMemoryCache(); 
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddBackgroundServices();
builder.Services.AddRefitClients(builder.Configuration);
builder.Services.AddIdempotencySupport();

builder.Services.AddRateLimiting(builder.Configuration);
builder.Services.AddCors(p => p.AddPolicy(CorsPolicyName, builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

builder.AddSerilog();


var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseCors(CorsPolicyName);

app.UseIpRateLimiting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UpdateDatabase<IceSyncContext>();

app.Run();