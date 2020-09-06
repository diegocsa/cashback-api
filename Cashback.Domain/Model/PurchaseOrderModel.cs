using System;

namespace Cashback.Domain.Model
{
    public class PurchaseOrderModel
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Status { get; set; }
        public ResellerModel Reseller { get; set; }
    }
}
