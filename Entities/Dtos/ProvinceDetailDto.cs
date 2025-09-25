using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ProvinceDetailDto : IDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Code { get; set; }
        public bool IsActive { get; set; }

        public int DistrictCount { get; set; }
        public int AddressCount { get; set; }
    }
}
