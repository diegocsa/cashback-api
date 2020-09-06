using System;

namespace Cashback.Domain.Model
{
    public class NewPurchaseOrderModel
    {
        public decimal Value { get; set; }
        public string CPF { get; set; }
        public DateTime Date { get; set; }
    }
}
