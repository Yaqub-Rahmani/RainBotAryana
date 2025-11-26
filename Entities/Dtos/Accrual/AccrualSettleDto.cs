using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.Accrual
{
    public class AccrualSettleDto: IDto
    {
        public Guid AccrualId { get; set; }
        public decimal Amount { get; set; }   // >= 0
        public string? Note { get; set; }
    }
}
