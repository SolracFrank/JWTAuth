using Application.Interfaces.AppServices;
using Application.Interfaces.Auth;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services.AppServices;
using Infrastructure.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToklenAPI.Data;
using ToklenAPI.Models.Dtos.JWTToken;

namespace Infrastructure
{
    public static class InfrastructureDependencyInjections
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //Add DBContext 
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                builder =>
                {
                    builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    builder.EnableRetryOnFailure(4, TimeSpan.FromSeconds(5), null);
                })
            );

            //configure appsettings / usersecrets
            services.Configure<JWTSettings>(configuration.GetSection("JwtSettings"));

            //Add services
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IIpAddressAccesorService, IpAddressAccesorService>();
            services.AddTransient<IAuthService, AuthService>();
        }
    }
}
