
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cdms.Common.Extensions;

public interface IValidatingOptions
{
    public bool Validate();
}

public static class OptionsExtensions
{
    private static OptionsBuilder<TOptions> CdmsValidation<TOptions>(this OptionsBuilder<TOptions>  options) where TOptions : class, IValidatingOptions
    {
        return options
            .ValidateDataAnnotations()
            .Validate(o => o.Validate())
            .ValidateOnStart();
    }
    
    public static OptionsBuilder<TOptions> CdmsAddOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, string section) where TOptions : class
    {
        var s = services
            .AddOptions<TOptions>()
            .Bind(configuration.GetSection(section))
            .ValidateDataAnnotations();
        
        if (typeof(IValidatingOptions).IsAssignableFrom(typeof(TOptions)))
        {
            s = s.Validate(o => ((IValidatingOptions)o).Validate())
                .ValidateOnStart();
        }

        return s;
    }
    
    // public static OptionsBuilder<TOptions> CdmsAddOptionsWithValidation<TOptions>(this IServiceCollection services, IConfiguration configuration, string section) where TOptions : class, IValidatingOptions
    // {
    //     return services
    //         .AddOptions<TOptions>()
    //         .Bind(configuration.GetSection(section))
    //         .ValidateDataAnnotations()  
    //         .CdmsValidation();
    // }
    
}