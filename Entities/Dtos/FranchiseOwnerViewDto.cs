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
        public string? Avatar { get; set; }            // MediaAsset.StorageUrl (Kind=Avatar, IsPrimary)
        public string? AddressFull { get; set; }       // Province + District + Line1
        public string? PrimaryPhone { get; set; }      // Phone.Number (IsPrimary)
        public decimal Balance { get; set; }           // FranchiseOwnerBalance.Balance
        public bool IsActive { get; set; }
    }
}
