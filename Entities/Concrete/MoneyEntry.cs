using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class MoneyEntry : IEntity
    {
        public Guid Id { get; set; }

        public Guid MoneyAccountId { get; set; }
        public BalanceDirection Direction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }        // Hesabın para biriminde (AFN / USD)

        public Currency Currency { get; set; }     // Hesabın para birimi

        // Hareket anındaki kur
        [Column(TypeName = "decimal(18,6)")]
        public decimal AfnPerUsdAtThatTime { get; set; }

        // USD karşılığı (rapor için)
        [Column(TypeName = "decimal(18,4)")]
        public decimal UsdEquivalent { get; set; }

        public DateTime OccurredAtUtc { get; set; }

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // İş bağları (opsiyonel)
        public Guid? ExpenseId { get; set; }
        public Guid? CollectionId { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ExpenseId))]
        public Expense? Expense { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(CollectionId))]
        public Collection? Collection { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(MoneyAccountId))]
        public MoneyAccount MoneyAccount { get; set; } = null!;
    }

}
