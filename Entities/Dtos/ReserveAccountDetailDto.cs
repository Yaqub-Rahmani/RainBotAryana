using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ReserveAccountDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "";     // Electricity / Market / Owner
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; }

        // Özet (ReserveEntry üzerinden)
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance => TotalIn - TotalOut;
    }
}
