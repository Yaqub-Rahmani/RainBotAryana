using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    //– کاتگوری را انتخاب کنید –
    //معاش کارمند
    //نان و چای کارمندان
    //مصارف روزانه مارکیت
    //خریداری جنس و لوازم
    //مصارف دفتر و اداری
    //بل‌ها: برق، آب، تلفن و انترنت
    //ترمیم، تعمیرات
    //مواد مصرفی و سوختنی
    //کرایه ترانسپورت
    //جریمه و فیس‌ها
    //دیگر مصارف
    public enum ExpenseCategory
    {
        Unknown = 0,

        Salary = 1,              // معاش کارمند
        StaffFood = 2,           // نان و چای کارمندان

        DailyMarketExpense = 10, // مصارف روزانه مارکیت 
        GoodsAndSupplies = 11,   // خریداری جنس و لوازم
        Office = 12,             // مصارف دفتر و اداری
        Utilities = 13,          // بل‌ها: برق، آب، تلفن و انترنت

        Maintenance = 20,        // ترمیم، تعمیرات
        Consumables = 30,        // مواد مصرفی و سوختنی
        TransportRent = 40,      // کرایه ترانسپورت 
        PenaltiesAndFees = 50,   // جریمه و فیس‌ها

        Other = 99               // دیگر مصارف
    }


    public enum ExpenseSource
    {
        Unknown = 0,
        MarketShare = 1,
        MarketReserve = 2,
        OwnerReserve = 3,
        ElectricityReserve = 4
    }

    public class Expense : IEntity
    {
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string DisplayName { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public Guid? MoneyEntryId { get; set; }

        public ExpenseCategory Category { get; set; }
        public ExpenseSource Source { get; set; }

        public Guid? EmployeeId { get; set; }

        public DateTime OccurredAtUtc { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Unknown;

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(EmployeeId))]
        public Employee Employee { get; set; } = null!;
    }

}
