using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public enum MoneyAccountType
    {
        CashLocal = 1,   // Yanımızdaki AFN
        CashUsd = 2,     // Yanımızdaki USD
        BankLocal = 3,   // Bankadaki AFN
        BankUsd = 4      // Bankadaki USD
    }

    public enum Currency
    {
        Afn = 1,
        Usd = 2
    }

    public class MoneyAccount : IEntity
    {
        public Guid Id { get; set; }
        public MoneyAccountType AccountType { get; set; }
        public Currency Currency { get; set; }
        public string Name { get; set; } = null!;  // "Kasa AFN", "Bank USD" gibi
        public bool IsActive { get; set; } = true;
    }

}
