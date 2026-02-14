using Core.Entities;
using Entities.Concrete;
using System;

namespace Entities.Dtos.Employee
{
    public class EmployeeDetailDto : IDto
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string FatherName { get; set; } = "";
        public Gender Gender { get; set; }
        public string NationalId { get; set; } = "";
        public bool IsActive { get; set; }

        // Work info snapshot (nullable)
        public string JobTitle { get; set; } = "";
        public decimal? SalaryAmount { get; set; }

        public string PhonePrimary { get; set; } = "";
        public string AvatarUrl { get; set; } = "";
    }

    public class EmployeeProfileDto : IDto
    {
        public string Id { get; set; } = "";

        public string DisplayName { get; set; } = "";
        public string FatherName { get; set; } = "";
        public Gender Gender { get; set; }
        public string NationalId { get; set; } = "";

        public bool IsActive { get; set; }

        public string PhonePrimary { get; set; } = "";
        public string PhoneSecondary { get; set; } = "";

        public string Province { get; set; } = "";
        public string District { get; set; } = "";
        public string AddressLine { get; set; } = "";
        public string AddressFull { get; set; } = "";

        public string AvatarUrl { get; set; } = "";
        public string IdFrontUrl { get; set; } = "";
        public string IdBackUrl { get; set; } = "";
        public string FingerBitmapUrl { get; set; } = "";

        // Work info (optional)
        public EmployeeWorkInfoDto? WorkInfo { get; set; }
    }

    public class EmployeeWorkInfoDto : IDto
    {
        public Guid EmployeeId { get; set; }
        public string JobTitle { get; set; } = "";
        public decimal? SalaryAmount { get; set; }
        public Currency SalaryCurrency { get; set; } = Currency.Afn;
        [System.ComponentModel.DataAnnotations.Range(1, 31)]
        public int PayDay { get; set; } = 1;

        [System.ComponentModel.DataAnnotations.Range(0, 999999999)]
        public decimal Interest { get; set; } = 0m;

        [System.ComponentModel.DataAnnotations.Range(0, 999999999)]
        public decimal Taxes { get; set; } = 0m;

        public string? Notes { get; set; }
        public bool IsActive { get; set; }
    }

    public class EmployeeWorkInfoUpsertDto
    {
        public string JobTitle { get; set; } = "";
        public decimal? SalaryAmount { get; set; }
        public Currency SalaryCurrency { get; set; } = Currency.Afn;

        [System.ComponentModel.DataAnnotations.Range(1, 31)]
        public int PayDay { get; set; } = 1;

        [System.ComponentModel.DataAnnotations.Range(0, 999999999)]
        public decimal Interest { get; set; } = 0m;

        [System.ComponentModel.DataAnnotations.Range(0, 999999999)]
        public decimal Taxes { get; set; } = 0m;

        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
