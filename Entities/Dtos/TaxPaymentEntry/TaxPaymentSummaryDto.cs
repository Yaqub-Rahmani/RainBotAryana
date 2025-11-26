using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.TaxPaymentEntry
{
    public class TaxPaymentSummaryDto
    {
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Net => TotalIn - TotalOut;
    }
}
