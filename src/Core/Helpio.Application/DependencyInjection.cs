using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Helpio.Ir.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // Register your application services here
            // Example:
            // services.AddScoped<IYourService, YourService>();

            // Register your validators here
            // Example:
            // services.AddScoped<IValidator<YourDto>, YourDtoValidator>();

            return services;
        }
    }
}