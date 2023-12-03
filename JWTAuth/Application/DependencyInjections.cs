using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class DependencyInjections
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(
                configuration => configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        }
    }
}
