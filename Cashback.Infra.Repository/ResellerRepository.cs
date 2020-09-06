using Cashback.Domain.Entities;
using Cashback.Domain.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cashback.Infra.Repository
{
    public class ResellerRepository : IResellerRepository
    {
        private readonly PrincipalContext _context;
        private DbSet<Reseller> _dbSet;

        public ResellerRepository(PrincipalContext context)
        {
            _context = context;
            _dbSet = _context.Set<Reseller>();
        }

        public async Task Add(Reseller reseller)
        {
            await _dbSet.AddAsync(reseller);
            await _context.SaveChangesAsync();
        }

        public IQueryable<Reseller> Retrieve(Expression<Func<Reseller, bool>> predicate = null)
        {
            IQueryable<Reseller> qry = _dbSet;
            if (predicate != null)
                qry = qry.Where(predicate);

            return qry;
        }
    }
}
