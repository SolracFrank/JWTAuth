using Application.Interfaces.AppServices;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.AppServices
{
    public  class IpAddressAccesorService : IIpAddressAccesorService
    {
        private readonly IHttpContextAccessor _httpAccesor;
        public  IpAddressAccesorService(IHttpContextAccessor httpAccesor)
        {
            _httpAccesor = httpAccesor;
        }
        public string GenerateIpAddress()
        {
            var httpContext = _httpAccesor.HttpContext;

            if (httpContext == null)
            {
                return "IP Unavailable";
            }

            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }
            else if (httpContext.Connection.RemoteIpAddress != null)
            {
                return httpContext.Connection.RemoteIpAddress.ToString();
            }

            return "IP Unavailable";
        }
    }
}
