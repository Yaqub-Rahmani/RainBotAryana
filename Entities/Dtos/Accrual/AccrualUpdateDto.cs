using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.Accrual
{
    public class AccrualUpdateDto : AccrualCreateDto
    {
        public Guid Id { get; set; }
        public AccrualStatus Status { get; set; }
        public decimal OutstandingAmount { get; set; }
    }
}
