
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Btms.Common.Extensions;

public interface IValidatingOptions
{
    public bool Validate();
}

public static class OptionsExtensions
{
    
    private static OptionsBuilder<TOptions> BtmsValidation<TOptions>(this OptionsBuilder<TOptions>  options) where TOptions : class, IValidatingOptions
    {
        return options
            .ValidateDataAnnotations()
            .Validate(o => o.Validate())
            .ValidateOnStart();
    }

    public static OptionsBuilder<TOptions> BtmsAddOptions<TOptions, TValidator>(this IServiceCollection services,
        IConfiguration configuration, string section)
        where TOptions : class where TValidator : class, IValidateOptions<TOptions>
    {
        return services
            .AddSingleton<IValidateOptions<TOptions>, TValidator>()
            .BtmsAddOptions<TOptions>(configuration, section);
    }

    public static OptionsBuilder<TOptions> BtmsAddOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, string section)
        where TOptions : class 
    {
        
        var s = services
            .AddOptions<TOptions>()
            .Bind(configuration.GetSection(section))
            .ValidateDataAnnotations();
        
        return s;
    }
}