using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.TaxPaymentEntry
{
    public class TaxPaymentEntryUpdateDto : TaxPaymentEntryCreateDto
    {
        public Guid Id { get; set; }
    }
}
