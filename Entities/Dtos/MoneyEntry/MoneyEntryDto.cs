using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.MoneyEntry
{
    public class MoneyEntryDto
    {
        public Guid Id { get; set; }

        public Guid MoneyAccountId { get; set; }
        public BalanceDirection Direction { get; set; }

        public decimal Amount { get; set; }
        public Currency Currency { get; set; }

        public decimal AfnPerUsdAtThatTime { get; set; }
        public decimal UsdEquivalent { get; set; }

        public DateTime OccurredAtUtc { get; set; }

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public Guid? ExpenseId { get; set; }
        public Guid? CollectionId { get; set; }
    }

    public class MoneyEntryCreateDto
    {
        public Guid Id { get; set; }
        public Guid MoneyAccountId { get; set; }
        public BalanceDirection Direction { get; set; }

        public decimal Amount { get; set; }
        public Currency Currency { get; set; }

        public Guid? ExpenseId { get; set; }
        public Guid? CollectionId { get; set; }
    }


}
