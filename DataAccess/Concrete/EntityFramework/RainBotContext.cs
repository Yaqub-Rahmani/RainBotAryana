using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.Concrete.EntityFramework
{
//dotnet ef migrations add updateEmpolyeeDediles -p.\DataAccess\DataAccess.csproj -c RainBotContext
//dotnet ef database update -p.\DataAccess\DataAccess.csproj -c RainBotContext

    public class RainBotContext : DbContext
    {
        public RainBotContext(DbContextOptions<RainBotContext> options) : base(options) { }
        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            builder.Properties<DateOnly>().HaveColumnType("date");
            builder.Properties<DateOnly?>().HaveColumnType("date");
        }

        public DbSet<FranchiseOwnerBalance> FranchiseOwnerBalances => Set<FranchiseOwnerBalance>();


        // ==== DbSets (ihtiyaçlarına göre düzenle) ====
        public DbSet<Market> Markets => Set<Market>();
        public DbSet<Floor> Floors => Set<Floor>();
        public DbSet<Shop> Shops => Set<Shop>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<FranchiseOwner> FranchiseOwners => Set<FranchiseOwner>();
        public DbSet<ShopFranchiseShare> ShopFranchiseShares => Set<ShopFranchiseShare>();
        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
        public DbSet<Accrual> Accruals => Set<Accrual>();
        public DbSet<AccrualSettlement> AccrualSettlements => Set<AccrualSettlement>();
        public DbSet<Collection> Collections => Set<Collection>();
        public DbSet<CollectionAllocation> CollectionAllocations => Set<CollectionAllocation>();
        public DbSet<ReserveAccount> ReserveAccounts => Set<ReserveAccount>();
        public DbSet<ReserveEntry> ReserveEntries => Set<ReserveEntry>();
        public DbSet<OwnerDistribution> OwnerDistributions => Set<OwnerDistribution>();
        public DbSet<OwnerDistributionLine> OwnerDistributionLines => Set<OwnerDistributionLine>();
        public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
        public DbSet<FamilyPayout> FamilyPayouts => Set<FamilyPayout>();
        public DbSet<FamilyPayoutLine> FamilyPayoutLines => Set<FamilyPayoutLine>();
        public DbSet<FamilyBalanceEntry> FamilyBalanceEntries => Set<FamilyBalanceEntry>();
        public DbSet<Province> Provinces => Set<Province>();
        public DbSet<District> Districts => Set<District>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<MarketMember> MarketMembers => Set<MarketMember>();
        public DbSet<SingleRatePolicy> SingleRatePolicies => Set<SingleRatePolicy>();
        public DbSet<OwnerDistributionPolicy> OwnerDistributionPolicies => Set<OwnerDistributionPolicy>();
        public DbSet<FamilySharePolicy> FamilySharePolicies => Set<FamilySharePolicy>();
        public DbSet<FranchiseBalanceEntry> FranchiseBalanceEntries => Set<FranchiseBalanceEntry>();

        public DbSet<TaxPaymentEntry> TaxPaymentEntries => Set<TaxPaymentEntry>();
        public DbSet<ZakatPaymentEntry> ZakatPaymentEntries => Set<ZakatPaymentEntry>();
        public DbSet<CharityPaymentEntry> CharityPaymentEntries => Set<CharityPaymentEntry>();

        public DbSet<DollarRate> DollarRates => Set<DollarRate>();
        public DbSet<MoneyAccount> MoneyAccounts => Set<MoneyAccount>();
        public DbSet<MoneyEntry> MoneyEntries => Set<MoneyEntry>();

        // ===== Employees =====
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<EmployeeWorkInfo> EmployeeWorkInfos => Set<EmployeeWorkInfo>();
        public DbSet<Expense> Expenses => Set<Expense>();


        public DbSet<User> Users => Set<User>();
        public DbSet<OperationClaim> OperationClaims => Set<OperationClaim>();
        public DbSet<UserOperationClaim> UserOperationClaims => Set<UserOperationClaim>();

        public DbSet<Biometric> Biometrics => Set<Biometric>();
        public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
        public DbSet<Phone> Phones => Set<Phone>();


        protected override void OnModelCreating(ModelBuilder model)
        {
            // ===== Employees =====
            model.Entity<Employee>(b =>
            {
                b.ToTable("Employee");
                b.HasKey(x => x.Id);
                b.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
                b.Property(x => x.FatherName).IsRequired().HasMaxLength(200);
                b.Property(x => x.NationalId).HasMaxLength(20);
                b.HasIndex(x => x.NationalId);
                b.HasIndex(x => x.IsActive);
            });

            model.Entity<Expense>(b =>
            {
                b.ToTable("Expense");
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.EmployeeId);
            });

            model.Entity<EmployeeTransaction>(b =>
            {
                b.ToTable("EmployeeTransactions");
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.EmployeeId);
                b.HasIndex(x => new { x.EmployeeId, x.Year, x.Month });
            });

            model.Entity<EmployeeWorkInfo>(b =>
            {
                b.ToTable("EmployeeWorkInfos");
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.EmployeeId).IsUnique();
                b.Property(x => x.JobTitle).HasMaxLength(200);
                b.Property(x => x.Notes).HasMaxLength(500);
                b.Property(x => x.SalaryAmount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Interest).HasColumnType("decimal(18,2)");
                b.Property(x => x.Taxes).HasColumnType("decimal(18,2)");

                b.HasOne(x => x.Employee)
                 .WithMany()
                 .HasForeignKey(x => x.EmployeeId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            model.Entity<Expense>(b =>
            {
                b.ToTable("Expense");
                b.HasIndex(x => new { x.EmployeeId, x.Category, x.OccurredAtUtc });
            });

            model.Entity<MediaAsset>(b =>
            {
                b.ToTable("MediaAssets");
                b.HasKey(x => x.Id);
                b.Property(x => x.StorageUrl).HasMaxLength(400).IsRequired();
                b.Property(x => x.HashChecksum).HasMaxLength(128);
                b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                b.HasIndex(x => new { x.OwnerType, x.OwnerId, x.Kind });
            });


            model.Entity<Phone>(b =>
            {
                b.ToTable("Phones");
                b.HasKey(x => x.Id);
                b.Property(x => x.Number).HasMaxLength(32).IsRequired();
                b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                // Aynı Owner için aynı Label tek olsun istiyorsan:
                // b.HasIndex(x => new { x.OwnerType, x.OwnerId, x.Label }).IsUnique();
            });


            model.Entity<Biometric>(b =>
            {
                b.ToTable("Biometrics");
                b.HasKey(x => x.Id);
                b.Property(x => x.Template).HasColumnType("varbinary(max)");
                b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });


            // ===== Market =====
            model.Entity<Market>(b =>
            {
                b.Property(x => x.MarketName).IsRequired().HasMaxLength(100);
                b.HasMany(x => x.Floors)
                 .WithOne(x => x.Market)
                 .HasForeignKey(x => x.MarketId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Shops)
                 .WithOne(x => x.Market)
                 .HasForeignKey(x => x.MarketId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Floor =====
            model.Entity<Floor>(b =>
            {
                b.HasIndex(x => new { x.MarketId, x.Level }).IsUnique();
                b.HasOne(x => x.Market)
                 .WithMany(x => x.Floors)
                 .HasForeignKey(x => x.MarketId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Shops)
                 .WithOne(x => x.Floor)
                 .HasForeignKey(x => x.FloorId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Shop =====
            model.Entity<Shop>(b =>
            {
                b.HasIndex(x => new { x.FloorId, x.UnitNumber }); // non-unique index
                b.HasOne(x => x.Market)
                 .WithMany(x => x.Shops)
                 .HasForeignKey(x => x.MarketId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Floor)
                 .WithMany(x => x.Shops)
                 .HasForeignKey(x => x.FloorId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Inverse’leri netleştir – gölge FK engeli
                b.HasMany(x => x.Contracts)
                 .WithOne(x => x.Shop)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.MeterReadings)
                 .WithOne(x => x.Shop)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Restrict); // Cascade yerine Restrict (geçmişi koru)

                b.HasMany(x => x.Accruals)
                 .WithOne(x => x.Shop)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Collections)
                 .WithOne(x => x.Shop)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Allocations)
                 .WithOne(x => x.Shop)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.ShopFranchiseShares)
                 .WithOne(x => x.Shop)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.FranchiseBalanceEntries)
                 .WithOne(x => x.Shop)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== FranchiseOwner & ShopFranchiseShare / FranchiseBalanceEntry =====
            model.Entity<FranchiseOwner>(b =>
            {
                b.HasMany(x => x.ShopFranchiseShares)
                 .WithOne(x => x.FranchiseOwner)
                 .HasForeignKey(x => x.FranchiseOwnerId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.FranchiseBalanceEntries)
                 .WithOne(x => x.FranchiseOwner)
                 .HasForeignKey(x => x.FranchiseOwnerId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<ShopFranchiseShare>(b =>
            {
                b.HasIndex(x => new { x.ShopId, x.FranchiseOwnerId, x.EffectiveFrom }).IsUnique();
                b.HasOne(x => x.Shop)
                 .WithMany(x => x.ShopFranchiseShares)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.FranchiseOwner)
                 .WithMany(x => x.ShopFranchiseShares)
                 .HasForeignKey(x => x.FranchiseOwnerId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            model.Entity<FranchiseBalanceEntry>(b =>
            {
                b.HasOne(x => x.FranchiseOwner)
                 .WithMany(x => x.FranchiseBalanceEntries)
                 .HasForeignKey(x => x.FranchiseOwnerId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Shop)
                 .WithMany(x => x.FranchiseBalanceEntries)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Collection)
                 .WithMany(x => x.FranchiseBalanceEntries)
                 .HasForeignKey(x => x.CollectionId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Tenant =====
            model.Entity<Tenant>(b =>
            {
                b.HasMany(x => x.Contracts)
                 .WithOne(x => x.Tenant)
                 .HasForeignKey(x => x.TenantId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Collections)
                 .WithOne(x => x.Tenant)
                 .HasForeignKey(x => x.TenantId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Accruals)
                 .WithOne(x => x.Tenant)
                 .HasForeignKey(x => x.TenantId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Allocations)
                 .WithOne(x => x.Tenant)
                 .HasForeignKey(x => x.TenantId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Contract =====
            model.Entity<Contract>(b =>
            {
                b.HasIndex(x => new { x.ShopId, x.TenantId, x.Status, x.StartDate, x.EndDate });
                b.HasOne(x => x.Shop)
                 .WithMany(x => x.Contracts)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Tenant)
                 .WithMany(x => x.Contracts)
                 .HasForeignKey(x => x.TenantId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Collections)
                 .WithOne(x => x.Contract)
                 .HasForeignKey(x => x.ContractId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Accruals)
                 .WithOne(x => x.Contract)
                 .HasForeignKey(x => x.ContractId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Allocations)
                 .WithOne(x => x.Contract)
                 .HasForeignKey(x => x.ContractId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== MeterReading =====
            model.Entity<MeterReading>(b =>
            {
                b.HasIndex(x => new { x.ShopId, x.Year, x.Month }).IsUnique();
                b.HasOne(x => x.Shop)
                 .WithMany(x => x.MeterReadings)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Accruals)
                 .WithOne(x => x.MeterReading)
                 .HasForeignKey(x => x.MeterReadingId)
                 .OnDelete(DeleteBehavior.SetNull);

                b.HasMany(x => x.Allocations)
                 .WithOne(x => x.MeterReading)
                 .HasForeignKey(x => x.MeterReadingId)
                 .OnDelete(DeleteBehavior.SetNull);

                b.HasMany(x => x.ReserveEntries)
                 .WithOne(x => x.MeterReading)
                 .HasForeignKey(x => x.MeterReadingId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Collection =====
            model.Entity<Collection>(b =>
            {
                b.HasOne(x => x.Shop)
                 .WithMany(x => x.Collections)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Tenant)
                 .WithMany(x => x.Collections)
                 .HasForeignKey(x => x.TenantId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Contract)
                 .WithMany(x => x.Collections)
                 .HasForeignKey(x => x.ContractId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Allocations)
                 .WithOne(x => x.Collection)
                 .HasForeignKey(x => x.CollectionId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.ReserveEntries)
                 .WithOne(x => x.Collection)
                 .HasForeignKey(x => x.CollectionId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.FranchiseBalanceEntries)
                 .WithOne(x => x.Collection)
                 .HasForeignKey(x => x.CollectionId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.FamilyBalanceEntries)
                 .WithOne(x => x.Collection)
                 .HasForeignKey(x => x.CollectionId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.OwnerDistributions)
                 .WithOne(x => x.Collection)
                 .HasForeignKey(x => x.CollectionId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CollectionAllocation =====
            model.Entity<CollectionAllocation>(b =>
            {
                b.HasOne(x => x.Collection)
                 .WithMany(x => x.Allocations)
                 .HasForeignKey(x => x.CollectionId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Contract)
                 .WithMany(x => x.Allocations)
                 .HasForeignKey(x => x.ContractId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Shop)
                 .WithMany(x => x.Allocations)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Tenant)
                 .WithMany(x => x.Allocations)
                 .HasForeignKey(x => x.TenantId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.MeterReading)
                 .WithMany(x => x.Allocations)
                 .HasForeignKey(x => x.MeterReadingId)
                 .OnDelete(DeleteBehavior.SetNull);

                b.HasMany(x => x.AccrualSettlements)
                 .WithOne(x => x.CollectionAllocation)
                 .HasForeignKey(x => x.CollectionAllocationId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.ReserveEntries)
                 .WithOne(x => x.CollectionAllocation)
                 .HasForeignKey(x => x.CollectionAllocationId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.OwnerDistributions)
                 .WithOne(x => x.CollectionAllocation)
                 .HasForeignKey(x => x.CollectionAllocationId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Accrual =====
            model.Entity<Accrual>(b =>
            {
                b.HasIndex(a => new { a.ShopId, a.TenantId, a.ContractId, a.Year, a.Month, a.Kind }).IsUnique();

                b.HasOne(x => x.Shop)
                 .WithMany(x => x.Accruals)
                 .HasForeignKey(x => x.ShopId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Tenant)
                 .WithMany(x => x.Accruals)
                 .HasForeignKey(x => x.TenantId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Contract)
                 .WithMany(x => x.Accruals)
                 .HasForeignKey(x => x.ContractId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.MeterReading)
                 .WithMany(x => x.Accruals)
                 .HasForeignKey(x => x.MeterReadingId)
                 .OnDelete(DeleteBehavior.SetNull);

                b.HasMany(x => x.AccrualSettlements)
                 .WithOne(x => x.Accrual)
                 .HasForeignKey(x => x.AccrualId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== AccrualSettlement =====
            model.Entity<AccrualSettlement>(b =>
            {
                b.HasIndex(x => new { x.AccrualId, x.CollectionAllocationId }).IsUnique();

                b.HasOne(x => x.Accrual)
                 .WithMany(x => x.AccrualSettlements)
                 .HasForeignKey(x => x.AccrualId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.CollectionAllocation)
                 .WithMany(x => x.AccrualSettlements)
                 .HasForeignKey(x => x.CollectionAllocationId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== ReserveAccount / ReserveEntry =====
            model.Entity<ReserveAccount>(b =>
            {
                b.HasIndex(x => x.Type).IsUnique(); // Gerek görmezsen kaldırabilirsin
                b.HasMany(x => x.ReserveEntries)
                 .WithOne(x => x.ReserveAccount)
                 .HasForeignKey(x => x.ReserveAccountId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            model.Entity<ReserveEntry>(b =>
            {
                b.HasOne(x => x.ReserveAccount)
                 .WithMany(x => x.ReserveEntries)
                 .HasForeignKey(x => x.ReserveAccountId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Collection)
                 .WithMany(x => x.ReserveEntries)
                 .HasForeignKey(x => x.CollectionId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.CollectionAllocation)
                 .WithMany(x => x.ReserveEntries)
                 .HasForeignKey(x => x.CollectionAllocationId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.MeterReading)
                 .WithMany(x => x.ReserveEntries)
                 .HasForeignKey(x => x.MeterReadingId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Contract)
                 .WithMany(x => x.ReserveEntries)
                 .HasForeignKey(x => x.ContractId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== OwnerDistribution / Lines & Family =====
            model.Entity<OwnerDistribution>(b =>
            {
                b.HasOne(x => x.Contract)
                 .WithMany(x => x.OwnerDistributions)
                 .HasForeignKey(x => x.ContractId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Collection)
                 .WithMany(x => x.OwnerDistributions)
                 .HasForeignKey(x => x.CollectionId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.CollectionAllocation)
                 .WithMany(x => x.OwnerDistributions)
                 .HasForeignKey(x => x.CollectionAllocationId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.OwnerDistributionLines)
                 .WithOne(x => x.OwnerDistribution)
                 .HasForeignKey(x => x.OwnerDistributionId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.FamilyPayouts)
                 .WithOne(x => x.OwnerDistribution)
                 .HasForeignKey(x => x.OwnerDistributionId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.FamilyBalanceEntries)
                 .WithOne(x => x.OwnerDistribution)
                 .HasForeignKey(x => x.OwnerDistributionId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<OwnerDistributionLine>(b =>
            {
                b.HasOne(x => x.OwnerDistribution)
                 .WithMany(x => x.OwnerDistributionLines)
                 .HasForeignKey(x => x.OwnerDistributionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            model.Entity<FamilyPayout>(b =>
            {
                b.HasOne(x => x.OwnerDistribution)
                 .WithMany(x => x.FamilyPayouts)
                 .HasForeignKey(x => x.OwnerDistributionId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.FamilyPayoutLines)
                 .WithOne(x => x.FamilyPayout)
                 .HasForeignKey(x => x.FamilyPayoutId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.FamilyBalanceEntries)
                 .WithOne(x => x.FamilyPayout)
                 .HasForeignKey(x => x.FamilyPayoutId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<FamilyPayoutLine>(b =>
            {
                b.HasOne(x => x.FamilyPayout)
                 .WithMany(x => x.FamilyPayoutLines)
                 .HasForeignKey(x => x.FamilyPayoutId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.FamilyMember)
                 .WithMany(x => x.FamilyPayoutLines) // .WithMany() yerine koleksiyon adı
                 .HasForeignKey(x => x.FamilyMemberId)
                 .OnDelete(DeleteBehavior.Restrict);
            });


            model.Entity<FamilyMember>(b =>
            {
                b.ToTable("FamilyMembers");
                b.HasKey(x => x.Id);
                b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
                b.Property(x => x.RelationTag).HasMaxLength(50);

                // DOĞRU: FamilyMember sadece FamilyPayoutLines koleksiyonuna sahip
                b.HasMany(x => x.FamilyPayoutLines)
                 .WithOne(x => x.FamilyMember)
                 .HasForeignKey(x => x.FamilyMemberId)
                 .OnDelete(DeleteBehavior.Restrict);

                // HATALI BLOK (SİL)
                // b.HasMany(x => x.FamilyBalanceEntries) ...
            });

            model.Entity<FamilyBalanceEntry>(b =>
            {
                b.ToTable("FamilyBalanceEntries");
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Notes).HasMaxLength(300);
                b.HasIndex(x => new { x.FamilyMemberId, x.OccurredAtUtc });

                // DOĞRU: FamilyMember tarafında inverse NAV yok => WithMany()
                b.HasOne(x => x.FamilyMember)
                 .WithMany()
                 .HasForeignKey(x => x.FamilyMemberId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Diğer referanslar: inverse NAV'lar ENTITY'lerde mevcut
                b.HasOne(x => x.FamilyPayout)
                 .WithMany(x => x.FamilyBalanceEntries)
                 .HasForeignKey(x => x.FamilyPayoutId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.OwnerDistribution)
                 .WithMany(x => x.FamilyBalanceEntries)
                 .HasForeignKey(x => x.OwnerDistributionId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Collection)
                 .WithMany(x => x.FamilyBalanceEntries)
                 .HasForeignKey(x => x.CollectionId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Contract)
                 .WithMany(x => x.FamilyBalanceEntries)
                 .HasForeignKey(x => x.ContractId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Province / District / Address =====
            model.Entity<Province>(b =>
            {
                b.HasMany(x => x.Districts)
                 .WithOne(x => x.Province)
                 .HasForeignKey(x => x.ProvinceId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Addresses)
                 .WithOne(x => x.Province)
                 .HasForeignKey(x => x.ProvinceId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<District>(b =>
            {
                b.HasIndex(x => new { x.ProvinceId, x.Name }).IsUnique();
                b.HasOne(x => x.Province)
                 .WithMany(x => x.Districts)
                 .HasForeignKey(x => x.ProvinceId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(x => x.Addresses)
                 .WithOne(x => x.District)
                 .HasForeignKey(x => x.DistrictId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<Address>(b =>
            {
                b.HasIndex(x => new { x.OwnerType, x.OwnerId, x.IsPrimary });
                b.Property(x => x.Street).HasMaxLength(200);

                b.HasOne(x => x.Province)
                 .WithMany(x => x.Addresses)
                 .HasForeignKey(x => x.ProvinceId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.District)
                 .WithMany(x => x.Addresses)
                 .HasForeignKey(x => x.DistrictId)
                 .OnDelete(DeleteBehavior.Restrict);
                // Polimorfik sahiplik (OwnerType+OwnerId) -> FK oluşturma yok
            });

            // ===== MarketMember / Policies (sadece temel alan & indeksler) =====
            model.Entity<MarketMember>(b =>
            {
                b.ToTable("MarketMembers");
                b.HasKey(x => x.Id);
                b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
                b.Property(x => x.MemberType).HasMaxLength(50);
                b.Property(x => x.IsActive).HasDefaultValue(true);

                // HATALI SATIRLAR (KALDIR):
                // b.HasIndex(x => new { x.MarketId, x.UserId }).IsUnique(false);
                // b.HasOne(x => x.Market).WithMany(x => x.Members)...
            });

            model.Entity<TaxPaymentEntry>(b =>
            {
                b.ToTable("TaxPaymentEntries");
                b.HasKey(x => x.Id);

                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Notes).HasMaxLength(300);

                b.Property(x => x.OccurredAtUtc).HasDefaultValueSql("GETUTCDATE()");
                b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                b.Property(x => x.PaymentReference).HasMaxLength(100);

                b.HasIndex(x => new { x.OwnerDistributionId, x.OccurredAtUtc });
                b.HasIndex(x => x.CollectionId);
                b.HasIndex(x => x.ContractId);
            });

            model.Entity<ZakatPaymentEntry>(b =>
            {
                b.ToTable("ZakatPaymentEntries");
                b.HasKey(x => x.Id);

                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Notes).HasMaxLength(300);

                b.Property(x => x.OccurredAtUtc).HasDefaultValueSql("GETUTCDATE()");
                b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                b.Property(x => x.PaymentReference).HasMaxLength(100);

                b.HasIndex(x => new { x.OwnerDistributionId, x.OccurredAtUtc });
                b.HasIndex(x => x.CollectionId);
                b.HasIndex(x => x.ContractId);
            });

            model.Entity<CharityPaymentEntry>(b =>
            {
                b.ToTable("CharityPaymentEntries");
                b.HasKey(x => x.Id);

                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Notes).HasMaxLength(300);

                b.Property(x => x.OccurredAtUtc).HasDefaultValueSql("GETUTCDATE()");
                b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                b.Property(x => x.PaymentReference).HasMaxLength(100);

                b.HasIndex(x => new { x.OwnerDistributionId, x.OccurredAtUtc });
                b.HasIndex(x => x.CollectionId);
                b.HasIndex(x => x.ContractId);
            });
        }
    }
}
