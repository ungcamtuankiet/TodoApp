
using Microsoft.Extensions.DependencyInjection;
using MRT.Application.Interfaces;
using MRT.Application.Services;
using MRT.Application.Utils;
namespace MRT.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<ICurrentTime, CurrentTime>();
            services.AddSingleton<TokenGenerators>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }
    }
}
