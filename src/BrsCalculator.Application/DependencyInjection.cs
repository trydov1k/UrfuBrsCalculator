using BrsCalculator.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BrsCalculator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<SubjectTemplateService>();
        services.AddScoped<WhatIfPlannerService>();
        return services;
    }
}
