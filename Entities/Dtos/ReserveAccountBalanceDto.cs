using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ReserveAccountBalanceDto : IDto
    {
        public Guid ReserveAccountId { get; set; }
        public string AccountTitle { get; set; } = "";
        public string AccountType { get; set; } = "";
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance => TotalIn - TotalOut;
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
    }
}
