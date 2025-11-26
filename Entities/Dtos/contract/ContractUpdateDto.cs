using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.Contract
{
    public class ContractUpdateDto : ContractCreateDto
    {
        public Guid Id { get; set; }
        public ContractStatus Status { get; set; }
        public string? ArchiveReason { get; set; }
    }
}
