using Application.Interfaces.AppServices;
using Application.Interfaces.Auth;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services.AppServices;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;
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
            services.AddTransient(typeof(IRefreshTokenService), typeof(RefreshTokenService));   
            services.AddTransient<IAuthService, AuthService>();

            //JWT Bearer Config
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("JwtExample", jwtOption =>
            {
                jwtOption.RequireHttpsMetadata = false;
                jwtOption.SaveToken = false;
                jwtOption.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    ValidIssuer = configuration["JWTSettings:Issuer"],
                    ValidAudience = configuration["JWTSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]))
                };

                jwtOption.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/plain";
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("token-Expired", "true");
                        }

                        return context.Response.WriteAsync(context.Exception.Message);
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(new UnAuthorizedException("Unauthorized"));
                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(new UnAuthorizedException("You have no permissions"));
                        return context.Response.WriteAsync(result);
                    }
                };
            });
        }
    }
}
