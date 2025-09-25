using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    //    Adresi tek bir yerde yönetin(en pratik)
    //Her varlık için ayrı ayrı adres alanları yerine tek bir Address tablosu kullan, hangi varlığa ait olduğunu Owner alanlarıyla belirt:
    public enum AddressOwnerType { Shop = 1, Tenant = 2, FranchiseOwner = 3, FamilyMember = 4 }

    public class Address : IEntity
    {
        public Guid Id { get; set; }

        // Sahiplik (polimorfik bağ)
        public AddressOwnerType OwnerType { get; set; }
        public Guid OwnerId { get; set; } // ShopId / TenantId / FranchiseOwnerId / FamilyMemberId

        // İl/İlçe
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }

        // Detay
        public string? Neighborhood { get; set; }  // Mahalle
        public string? Street { get; set; }        // Cadde/Sokak
        public string? BuildingNo { get; set; }    // Bina/No
        public string? UnitNo { get; set; }        // Daire
        public string? PostalCode { get; set; }
        public string? Notes { get; set; }

        // Varsayılan adres flag’i (bir varlığın birden çok adresi olabilir)
        public bool IsPrimary { get; set; } = true;

        // Nav (istenirse)
        public Province? Province { get; set; }

        public District? District { get; set; }

    }

}
