using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    //    FamilyMember(Aile Üyesi)
    //Aile fertleri için hesap sahibi kimliği. (Sadece market payından gelen dağıtım görünür.)

    public enum Gender { Unknown = 0, Male = 1, Female = 2 }

    public class FamilyMember : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string DisplayName { get; set; } = default!;

        public string FatherName { get; set; }

        public Gender Gender { get; set; } = Gender.Male;

        [MaxLength(20)]
        public string NationalId { get; set; }

        /// <summary>Örn. Parent, Daughter, Son, Spouse… (rapor filtresi için basit etiket)</summary>
        [MaxLength(50)]
        public string? RelationTag { get; set; }

        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        [ValidateNever]
        public ICollection<FamilyPayoutLine> FamilyPayoutLines { get; set; } = new List<FamilyPayoutLine>();
    }
}
