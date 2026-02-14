using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class FranchiseOwnerViewDto : IDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string? Avatar { get; set; }            
        public string? AddressFull { get; set; }      
        public string? PrimaryPhone { get; set; }      
        public decimal Balance { get; set; }          
        public bool IsActive { get; set; }
    }
}
