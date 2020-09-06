using Cashback.Domain.Model;
using System.Threading.Tasks;

namespace Cashback.Domain.Interfaces.Service
{
    public interface IAuthenticationService
    {
        Task<string> Login(LoginModel model);
    }
}
