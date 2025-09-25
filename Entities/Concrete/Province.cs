using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class Province : IEntity
    {
        public int Id { get; set; }

        // Örn: "Kabul"
        public string Name { get; set; } = null!;

        // Örn: "KBL" gibi kısa kod; opsiyonel
        public string? Code { get; set; }

        public bool IsActive { get; set; } = true;


        // İLİN İLÇELERİ
        public ICollection<District> Districts { get; set; } = new List<District>();

        // İLE BAĞLI ADRESLER
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
