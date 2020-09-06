using Cashback.Domain.Entities;
using Cashback.Domain.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cashback.Infra.Repository
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly PrincipalContext _context;
        private DbSet<PurchaseOrder> _dbSet;

        public PurchaseOrderRepository(PrincipalContext context)
        {
            _context = context;
            _dbSet = _context.Set<PurchaseOrder>();
        }

        public async Task Add(PurchaseOrder purchase)
        {
            await _dbSet.AddAsync(purchase);
            await _context.SaveChangesAsync();
        }

        public IQueryable<PurchaseOrder> Retrieve(Expression<Func<PurchaseOrder, bool>> predicate = null)
        {
            IQueryable<PurchaseOrder> qry = _dbSet.Include(res => res.Reseller);
            if (predicate != null)
                qry = qry.Where(predicate);

            return qry;
        }

    }
}
