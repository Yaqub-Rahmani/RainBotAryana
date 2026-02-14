using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.DollarRate
{
    public class DollarRateDto
    {
        public int Id { get; set; }
        public DateTime EffectiveFromUtc { get; set; }
        public decimal AfnPerUsd { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
