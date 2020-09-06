using Cashback.Domain.Interfaces.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Cashback.Service.Configuration
{
    public static class ExtensionsServiceConfiguration
    {
        public static IServiceCollection ConfigureCashbackServices(this IServiceCollection services)
        {
            services.AddScoped<IResellerService, ResellerService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<ICalculatorRuleService, CalculatorRuleService>();
            
            return services;
        }
    }
}
