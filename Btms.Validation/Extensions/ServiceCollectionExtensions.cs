using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Btms.Validation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBtmsValidation(this IServiceCollection services, Action<ValidationSetup> setup)
    {
        services.AddSingleton<IBtmsValidator, BtmsValidator>();

        setup(new ValidationSetup(services));

        return services;
    }
}

public class ValidationSetup(IServiceCollection services)
{
    public ValidationSetup AddValidatorsFromAssemblyContaining<T>()
    {
        services.AddValidatorsFromAssemblyContaining<T>(ServiceLifetime.Singleton);
        return this;
    }
}