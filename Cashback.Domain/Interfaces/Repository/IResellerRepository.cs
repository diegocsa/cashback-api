using Cashback.Domain.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cashback.Domain.Interfaces.Repository
{
    public interface IResellerRepository
    {
        Task Add(Reseller reseller);
        IQueryable<Reseller> Retrieve(Expression<Func<Reseller, bool>> predicate = null);
    }
}
