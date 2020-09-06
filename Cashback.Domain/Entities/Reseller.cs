using System;

namespace Cashback.Domain.Entities
{
    public class Reseller
    {
        public Guid Id { get; set; }
        public string CPF { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool AutoApproved { get; set; }
    }
}
