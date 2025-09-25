using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{

    public class ReserveAccountSummaryDto : IDto
    {
        public Guid? AccountId { get; set; }
        public string GroupKey { get; set; } = "";   // Hesap adı ya da Type
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance => TotalIn - TotalOut;
    }
}
