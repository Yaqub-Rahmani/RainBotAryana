using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Entities.Dtos.PaymentOutflow
{
    public enum OutflowType
    {
        FranchiseOwnerPayout = 1,
        Expense = 2,
        TaxPaymentEntry = 3,
        CharityPaymentEntry = 4,
        ReserveEntry = 5,
        FamilyMemberPayout = 6,
        EmployeeSalary = 7,
    }

    /// <summary>
    /// React payload'un birebir karşılığı + finans alanları.
    /// </summary>
    public class PaymentOutflowCreateFromUiDto
    {
        public Guid EntryId { get; set; }

        public PaymentMethod Method { get; set; } 

        public Guid OwnerId { get; set; }
        public decimal? Amount { get; set; }

        public string? Reference { get; set; }
        public string? Description { get; set; }

        public string? FingerprintTemplateBase64 { get; set; }
        public string? FingerprintBitmapPngBase64 { get; set; }

        public Currency Currency { get; set; }

        public OutflowType OutflowType { get; set; } = OutflowType.FranchiseOwnerPayout;


    }

    public class PaymentOutflowCreateResultDto
    {
        public Guid PaymentId { get; set; }
        public Guid MoneyEntryId { get; set; }
        public Guid DomainEntryId { get; set; }

        public string? FingerprintFolderUrl { get; set; }
        public string? FingerprintBitmapPath { get; set; }
        public Guid? FingerprintMediaAssetId { get; set; }
        public Guid? BiometricId { get; set; }
    }

}
