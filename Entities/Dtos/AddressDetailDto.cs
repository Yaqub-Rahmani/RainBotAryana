using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class AddressDetailDto : IDto
    {
        public Guid Id { get; set; }

        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; } = "";

        public int DistrictId { get; set; }
        public string DistrictName { get; set; } = "";

        public string? Neighborhood { get; set; }
        public string? Street { get; set; }
        public string? BuildingNo { get; set; }
        public string? UnitNo { get; set; }
        public string? PostalCode { get; set; }
        public string? Notes { get; set; }
        public bool IsPrimary { get; set; }

        // Polimorfik sahiplik özeti
        public string OwnerType { get; set; } = "";
        public Guid OwnerId { get; set; }

        // Görsel tam satır (UI için)
        public string DisplayLine =>
            string.Join(" | ", new[]{
                Neighborhood, Street, BuildingNo, UnitNo, DistrictName, ProvinceName, PostalCode
            }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
