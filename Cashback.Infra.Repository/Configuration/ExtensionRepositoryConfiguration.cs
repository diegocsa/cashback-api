using Cashback.Domain.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Cashback.Infra.Repository.Configuration
{
    public static class ExtensionRepositoryConfiguration
    {
        public static IServiceCollection ConfigureCashbackRepositories(this IServiceCollection services)
        {
            services.AddScoped<IResellerRepository, ResellerRepository>();
            services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            return services;
        }
    }
}
