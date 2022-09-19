using FluentValidation.AspNetCore;
using IceSync.Domain.Exceptions;
using IceSync.Domain.Exceptions.Custom;
using IceSync.Infrastructure.Validation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IceSync.API.Extensions.Configuration;

public static class MvcBuilderExtensions
{

    public static void AddFluentValidation(this IMvcBuilder builder)
    {
        builder.AddFluentValidation(options =>
        {
            options.RegisterValidatorsFromAssemblyContaining<GetWorkflowExecutionsValidator>();
            options.ImplicitlyValidateChildProperties = true;
        }).ConfigureApiBehaviorOptions(opts => opts.InvalidModelStateResponseFactory = context =>
        {
            var pattern = @"(?<=[A-Za-z])(?=[A-Z][a-z])|(?<=[a-z0-9])(?=[0-9]?[A-Z])";
            var errors = context.ModelState
                .Select(kvp => new { kvp.Key, kvp.Value.Errors })
                .SelectMany(a => a.Errors.Select(x =>
                    x.ErrorMessage.Replace("''", $"'{Regex.Replace(a.Key, pattern, " ")}'", StringComparison.CurrentCulture)))
                .ToList();

            var httpRequestPath = context.HttpContext.Request.Path;

            return new UnprocessableEntityObjectResult(Rfc7807.Factory<UnprocessableEntityDomainException>(httpRequestPath, errors));
        });
    }
}
