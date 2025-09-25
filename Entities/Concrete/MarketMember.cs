using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    //    (İsteğe bağlı) MarketMember(Aile/Paydaş Temeli)
    //Aile fertleri ve ileride “kim neyi görecek” yetkileri için basit bir temel.Şimdilik opsiyonel;
    //market sahibi ve aile bireyleri için bir çatı varlık oluşturmak istersen ekleyebiliriz.
    //İlerleyen adımda “Market Sahibi İç Dağıtım” tablolarına bağlamak için faydalı olur.
    public class MarketMember : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Ad Soyad</summary>
        [Required, MaxLength(200)]
        public string DisplayName { get; set; } = default!;

        /// <summary>Üye tipi (örn. Owner, Parent, Daughter, Son...) — Enum'a çevrilebilir</summary>
        [MaxLength(50)]
        public string? MemberType { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
