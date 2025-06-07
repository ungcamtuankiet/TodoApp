using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MRT.Application;
using MRT.Application.Interfaces;
using MRT.Application.Repositories;
using MRT.Application.Services;
using MRT.Contract.Abstractions.CloudinaryService;
using MRT.Infrastructure.Data;
using MRT.Infrastructure.Repositories;
using MRT.Infrastructure.Services;
using StackExchange.Redis;

namespace MRT.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //UOW
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //Services
            services.AddScoped<IRedisService, RedisService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddMemoryCache();
            //Repositories
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            //Database
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            );

            //Cloundinary
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            

            var redisConnectionString = configuration.GetConnectionString("Redis");
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConnectionString));
            return services;
        }
    }
}
