using Cashback.Domain.Entities;
using Cashback.Domain.Model;
using System.Threading.Tasks;

namespace Cashback.Domain.Interfaces.Service
{
    public interface IResellerService
    {
        Task<Reseller> Add(NewResellerModel model);
        Task<double> GetAccumulated(string cpf);
    }
}
