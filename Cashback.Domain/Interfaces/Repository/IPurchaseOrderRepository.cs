using Cashback.Domain.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cashback.Domain.Interfaces.Repository
{
    public interface IPurchaseOrderRepository
    {
        Task Add(PurchaseOrder purchase);
        IQueryable<PurchaseOrder> Retrieve(Expression<Func<PurchaseOrder, bool>> predicate = null);
    }
}
