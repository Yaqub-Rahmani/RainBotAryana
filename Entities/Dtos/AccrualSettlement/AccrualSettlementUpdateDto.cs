using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.AccrualSettlement
{
    public class AccrualSettlementUpdateDto:IDto
    {
        public Guid Id { get; set; }
        public string? Notes { get; set; }
    }
}
