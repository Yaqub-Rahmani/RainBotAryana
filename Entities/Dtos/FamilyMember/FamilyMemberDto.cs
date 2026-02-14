using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.FamilyMember
{
    public class FamilyMemberDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = default!;
        public string? FatherName { get; set; }
        public Gender Gender { get; set; }
        public string? NationalId { get; set; }
        public string? RelationTag { get; set; }
        public bool IsActive { get; set; }
        public bool IsParent { get; set; }
        public bool IncludeInShare { get; set; }
    }

    public class FamilyMemberCreateDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = default!;
        public string? FatherName { get; set; }
        public Gender Gender { get; set; } = Gender.Male;
        public string? NationalId { get; set; }
        public bool IsParent { get; set; } = true;
    }

    public class FamilyMemberUpdateDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = default!;
        public string? FatherName { get; set; }
        public Gender Gender { get; set; } = Gender.Male;
        public string? NationalId { get; set; }
        public string? RelationTag { get; set; }
        public bool IsParent { get; set; } = true;
        public bool IncludeInShare { get; set; } = true;
        public bool IsActive { get; set; } = true;
    }
}
