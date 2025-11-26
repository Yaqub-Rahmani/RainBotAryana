using Core.Entities;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.FranchiseOwner
{
    public class OwnerProfileDto: IDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string FatherName { get; set; }
        public Gender Gender { get; set; }
        public string NationalId { get; set; }

        public string PhonePrimary { get; set; }
        public string PhoneSecondary { get; set; }

        public string Province { get; set; }
        public string District { get; set; }
        public string AddressLine { get; set; }
        public string AddressFull { get; set; }

        public string Description { get; set; }

        public decimal Balance { get; set; }

        // Media
        public string AvatarUrl { get; set; }
        public string IdFrontUrl { get; set; }
        public string IdBackUrl { get; set; }
        public string FingerBitmapUrl { get; set; }
        public string ContractUrl { get; set; }
    }
}
