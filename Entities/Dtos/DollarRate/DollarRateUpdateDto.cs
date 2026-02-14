using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.DollarRate
{
    public class DollarRateUpdateDto
    {
        public int Id { get; set; }
        public DateTime EffectiveFromUtc { get; set; }
        public decimal AfnPerUsd { get; set; }
    }
}
