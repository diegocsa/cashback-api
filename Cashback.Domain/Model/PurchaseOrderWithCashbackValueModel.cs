using System;

namespace Cashback.Domain.Model
{
    public class PurchaseOrderWithCashbackValueModel
    {
        public Guid Id { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
        public decimal CashbackPercentual { get; set; }
        public decimal CashbackValue { get; set; }
        public string Status { get; set; }
    }
}
