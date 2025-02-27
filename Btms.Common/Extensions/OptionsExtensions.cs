using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Btms.Common.Extensions;

public static class OptionsExtensions
{
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

    public static TOptions Get<TOptions>(this OptionsBuilder<TOptions> optionsBuilder)
        where TOptions : class
    {
        return optionsBuilder.Services.BuildServiceProvider().GetRequiredService<IOptions<TOptions>>().Value;
    }
}