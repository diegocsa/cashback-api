using Cashback.Domain.Entities;
using Cashback.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cashback.Domain.Interfaces.Service
{
    public interface IPurchaseOrderService
    {
        Task<PurchaseOrder> Add(NewPurchaseOrderModel model);
        IEnumerable<PurchaseOrderWithCashbackValueModel> Get(string cpf, DateTime? start, DateTime? end);
    }
}
