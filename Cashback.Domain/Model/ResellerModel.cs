using System;

namespace Cashback.Domain.Model
{
    public class ResellerModel
    {
        public Guid Id { get; set; }
        public string CPF { get; set; } 
        public string Name { get; set; }
        public string Email { get; set; }
        
    }
}
