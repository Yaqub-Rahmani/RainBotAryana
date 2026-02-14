using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.DollarRate
{
    public class DollarRateCreateDto
    {
        public DateTime EffectiveFromUtc { get; set; }
        public decimal AfnPerUsd { get; set; }
    }
}
