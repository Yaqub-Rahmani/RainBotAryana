using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class DistrictDetailDto : IDto
    {
        public int Id { get; set; }
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Code { get; set; }
        public bool IsActive { get; set; }

        // ilişkisel özetler
        public int AddressCount { get; set; }
    }
}
