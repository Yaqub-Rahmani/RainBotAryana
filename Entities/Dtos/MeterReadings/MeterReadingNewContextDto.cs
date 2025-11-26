using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.MeterReadings
{
    public sealed class MeterReadingNewContextDto: IDto
    {
        public bool IsFirstReadingForContract { get; init; }

        // İstersen son okumanın özet bilgileri
        public decimal? LastUnitPrice { get; init; }
        public decimal? LastNetAmount { get; init; }
        public decimal? LastCurrentIndex { get; init; }
        public decimal? LastPreviouskWh { get; init; }
        public int? LastYear { get; init; }
        public int? LastMonth { get; init; }
    }

}
