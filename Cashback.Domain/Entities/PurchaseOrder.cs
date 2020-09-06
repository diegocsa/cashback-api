using System;

namespace Cashback.Domain.Entities
{
    public class PurchaseOrder
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Status { get; set; }
        
        public Guid ResellerId { get; set; }
        public virtual Reseller Reseller { get; set; }
        
    }
}
