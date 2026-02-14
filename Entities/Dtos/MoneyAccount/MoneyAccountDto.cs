using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.MoneyAccount
{
    public class MoneyAccountDto
    {
        public Guid Id { get; set; }
        public MoneyAccountType AccountType { get; set; }
        public Currency Currency { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
    }
    public class MoneyAccountCreateDto
    {
        public MoneyAccountType AccountType { get; set; }
        public Currency Currency { get; set; }
        public string Name { get; set; } = null!;
    }

    public class MoneyAccountUpdateDto
    {
        public Guid Id { get; set; }
        public MoneyAccountType AccountType { get; set; }
        public Currency Currency { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
