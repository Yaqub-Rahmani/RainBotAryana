using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.Concrete.EntityFramework
{
    public class RainBotContext : DbContext
    {
        public RainBotContext(DbContextOptions<RainBotContext> options) : base(options) { }

        // === EK: Çekirdek varlıklar ===
        public DbSet<Shop> Shops => Set<Shop>();
        public DbSet<FranchiseOwner> FranchiseOwners => Set<FranchiseOwner>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<ShopFranchiseShare> ShopFranchiseShares => Set<ShopFranchiseShare>();
        public DbSet<MarketMember> MarketMembers => Set<MarketMember>();


        // === EK: Policies ===
        public DbSet<SingleRatePolicy> SingleRatePolicies => Set<SingleRatePolicy>();
        public DbSet<OwnerDistributionPolicy> OwnerDistributionPolicies => Set<OwnerDistributionPolicy>();
        public DbSet<FamilySharePolicy> FamilySharePolicies => Set<FamilySharePolicy>();

        // === EK: Sözleşme ===
        public DbSet<Contract> Contracts => Set<Contract>();

        // === EK: Elektrik sayaç okumaları ===
        public DbSet<MeterReading> MeterReadings => Set<MeterReading>();

        // === EK: Tahsilat & dağıtım ===
        public DbSet<Collection> Collections => Set<Collection>();
        public DbSet<CollectionAllocation> CollectionAllocations => Set<CollectionAllocation>();

        // === EK: Tahakkuk/borç ===
        public DbSet<Accrual> Accruals => Set<Accrual>();
        public DbSet<AccrualSettlement> AccrualSettlements => Set<AccrualSettlement>();

        // === EK: Rezervler ===
        public DbSet<ReserveAccount> ReserveAccounts => Set<ReserveAccount>();
        public DbSet<ReserveEntry> ReserveEntries => Set<ReserveEntry>();

        // === EK: Market sahibi iç dağıtım ===
        public DbSet<OwnerDistribution> OwnerDistributions => Set<OwnerDistribution>();
        public DbSet<OwnerDistributionLine> OwnerDistributionLines => Set<OwnerDistributionLine>();

        // === EK: Aile & imtiyaz bakiyeleri ===
        public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
        public DbSet<FamilyPayout> FamilyPayouts => Set<FamilyPayout>();
        public DbSet<FamilyPayoutLine> FamilyPayoutLines => Set<FamilyPayoutLine>();
        public DbSet<FranchiseBalanceEntry> FranchiseBalanceEntries => Set<FranchiseBalanceEntry>();
        public DbSet<FamilyBalanceEntry> FamilyBalanceEntries => Set<FamilyBalanceEntry>();

        // === EK: Adres ===
        public DbSet<Address> Addresses => Set<Address>();


        public DbSet<Product> Products { get; set; }
        //public DbSet<Category> Categories { get; set; }
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Market> Markets { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Province> Provinces => Set<Province>();
        public DbSet<District> Districts => Set<District>();

        public DbSet<User> Users => Set<User>();
        public DbSet<OperationClaim> OperationClaims => Set<OperationClaim>();
        public DbSet<UserOperationClaim> UserOperationClaims => Set<UserOperationClaim>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Markets tablo & kolon ayarları
            modelBuilder.Entity<Market>().ToTable("Markets");
            modelBuilder.Entity<Market>()
                .Property(x => x.MarketName)
            .HasMaxLength(100)
            .IsRequired();

//dotnet ef migrations add InitialCreate--project DataAccess --startup - project WebAPI--context RainBotContext
//dotnet ef database update --project DataAccess--startup - project WebAPI--context RainBotContext

            //dotnet ef migrations add ShopSchemaInit--project DataAccess --startup - project WebAPI
            //dotnet ef database update--project DataAccess --startup - project WebAPI



            // (Opsiyonel) MarketName benzersiz olsun istersen:
            // modelBuilder.Entity<Markets>()
            //     .HasIndex(x => x.MarketName)
            //     .IsUnique();

            // Category: veritabanındaki kolon adı "Name" ise,
            // entity'deki CategoryName'i o kolona eşleyelim
            modelBuilder.Entity<Category>()
                .Property(x => x.CategoryName)
                .HasColumnName("Name");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Floor>(b =>
            {
                b.ToTable("Floors");
                b.HasKey(x => x.Id);
                b.Property(x => x.FloorName).HasMaxLength(100).IsRequired();
                b.HasIndex(x => new { x.MarketId, x.Level }).IsUnique(); // aynı markette aynı Level tek olsun
                b.HasOne<Market>()                                      // nav kullanmıyorsak tip belirt
                 .WithMany()
                 .HasForeignKey(x => x.MarketId)
                 .OnDelete(DeleteBehavior.Restrict); // market silinince engelle (istiyorsan Cascade yap)
            });

            // Province (önceden eklemiştik)
            modelBuilder.Entity<Province>(b =>
            {
                b.ToTable("Provinces");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(120).IsRequired();
                b.Property(x => x.Code).HasMaxLength(10);
                b.Property(x => x.IsActive).HasDefaultValue(true);
                b.HasIndex(x => x.Name).IsUnique(); // tekillik
            });

            // District
            modelBuilder.Entity<District>(b =>
            {
                b.ToTable("Districts");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(120).IsRequired();
                b.Property(x => x.Code).HasMaxLength(10);
                b.Property(x => x.IsActive).HasDefaultValue(true);

                b.HasOne(x => x.Province)                      // <- matches property name
                 .WithMany(p => p.Districts)                   // ensure Province has ICollection<District> Districts { get; set; }
                 .HasForeignKey(x => x.ProvinceId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(x => new { x.ProvinceId, x.Name }).IsUnique();
                b.HasIndex(x => x.ProvinceId);
            });

            // === EK: Shop ===
            modelBuilder.Entity<Shop>(b =>
            {
                b.ToTable("Shops");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(200);
                b.Property(x => x.UnitNumber).HasMaxLength(50);

                // ✅ İndeks: aynı kattaki aynı UnitNumber tek olsun isterseniz Unique yapabilirsiniz
                b.HasIndex(x => new { x.FloorId, x.UnitNumber }); // .IsUnique(); // isterse aç

                // ✅ İlişki: Shop -> Floor (çok - bire)
                b.HasOne(s => s.Floor)
                 .WithMany()                      // eğer Floor tarafında Shops koleksiyonu yoksa
                 .HasForeignKey(s => s.FloorId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
            // === EK: FranchiseOwner ===
            modelBuilder.Entity<FranchiseOwner>(b =>
            {
                b.ToTable("FranchiseOwners");
                b.HasKey(x => x.Id);
                b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
                b.Property(x => x.Phone).HasMaxLength(50);
                b.Property(x => x.Email).HasMaxLength(200);
                b.Property(x => x.Notes).HasMaxLength(1000);
            });

            // === EK: Tenant ===
            modelBuilder.Entity<Tenant>(b =>
            {
                b.ToTable("Tenants");
                b.HasKey(x => x.Id);
                b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
                b.Property(x => x.Phone).HasMaxLength(50);
                b.Property(x => x.Email).HasMaxLength(200);
                b.Property(x => x.TaxOrIdNumber).HasMaxLength(50);
            });

            // === EK: ShopFranchiseShare (tarihçeli oran) ===
            modelBuilder.Entity<ShopFranchiseShare>(b =>
            {
                b.ToTable("ShopFranchiseShares");
                b.HasKey(x => x.Id);
                b.Property(x => x.SharePercent).HasColumnType("decimal(5,2)");
                b.Property(x => x.Notes).HasMaxLength(500);

                b.HasIndex(x => new { x.ShopId, x.FranchiseOwnerId, x.EffectiveFrom }).IsUnique();
                b.HasOne<Shop>().WithMany(s => s.FranchiseShares).HasForeignKey(x => x.ShopId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne<FranchiseOwner>().WithMany(f => f.ShopShares).HasForeignKey(x => x.FranchiseOwnerId).OnDelete(DeleteBehavior.Cascade);
            });

            // === EK: Contract ===

            modelBuilder.Entity<Contract>(b =>
            {
                b.ToTable("Contracts");
                b.HasKey(x => x.Id);

                b.Property(x => x.OwnerPortionAmount).HasColumnType("decimal(18,2)");
                b.Property(x => x.FranchisePortionAmount).HasColumnType("decimal(18,2)");
                b.Property(x => x.MarketPortionAmount).HasColumnType("decimal(18,2)");
                b.Property(x => x.ElectricityNote).HasMaxLength(500);
                b.Property(x => x.Version).HasMaxLength(20);
                b.Property(x => x.ArchiveReason).HasMaxLength(500);

                b.HasIndex(x => new { x.ShopId, x.TenantId, x.Status, x.StartDate, x.EndDate });

                // Shop (1) -> Contracts (N)
                b.HasOne(c => c.Shop)
                 .WithMany(s => s.Contracts)
                 .HasForeignKey(c => c.ShopId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Tenant (1) -> Contract (1), FK: Contract.TenantId
                b.HasOne(c => c.Tenant)
                 .WithOne(t => t.Contract)
                 .HasForeignKey<Contract>(c => c.TenantId)
                 .OnDelete(DeleteBehavior.Restrict);

                // 1-1 ilişki için FK benzersiz:
                b.HasIndex(c => c.TenantId).IsUnique();
            });


            //modelBuilder.Entity<Contract>(b =>
            //{
            //    b.ToTable("Contracts");
            //    b.HasKey(x => x.Id);

            //    b.Property(x => x.OwnerPortionAmount).HasColumnType("decimal(18,2)");
            //    b.Property(x => x.FranchisePortionAmount).HasColumnType("decimal(18,2)");
            //    b.Property(x => x.MarketPortionAmount).HasColumnType("decimal(18,2)");
            //    b.Property(x => x.ElectricityNote).HasMaxLength(500);
            //    b.Property(x => x.Version).HasMaxLength(20);
            //    b.Property(x => x.ArchiveReason).HasMaxLength(500);

            //    b.HasIndex(x => new { x.ShopId, x.TenantId, x.Status, x.StartDate, x.EndDate });


            //    b.HasOne<Shop>().WithMany().HasForeignKey(x => x.ShopId);
            //    b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId);


            //});

            // === EK: MeterReading ===
            modelBuilder.Entity<MeterReading>(b =>
            {
                b.ToTable("MeterReadings");
                b.HasKey(x => x.Id);

                b.Property(x => x.PreviousIndex).HasColumnType("decimal(18,3)");
                b.Property(x => x.CurrentIndex).HasColumnType("decimal(18,3)");
                b.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)");
                b.Property(x => x.Notes).HasMaxLength(500);

                b.HasIndex(x => new { x.ShopId, x.Year, x.Month }).IsUnique();
                b.HasOne<Shop>().WithMany().HasForeignKey(x => x.ShopId).OnDelete(DeleteBehavior.Cascade);
            });

            // === EK: Collection (tahsilat) ===
            modelBuilder.Entity<Collection>(b =>
            {
                b.ToTable("Collections");
                b.HasKey(x => x.Id);
                b.Property(x => x.AmountReceived).HasColumnType("decimal(18,2)");
                b.Property(x => x.ReferenceNo).HasMaxLength(100);
                b.Property(x => x.Notes).HasMaxLength(500);

                b.HasIndex(x => new { x.ShopId, x.TenantId, x.ContractId, x.PaidAtUtc });
                b.HasOne<Shop>().WithMany().HasForeignKey(x => x.ShopId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<Contract>().WithMany().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
            });

            // === EK: CollectionAllocation (tahsilat dağıtım satırları) ===
            modelBuilder.Entity<CollectionAllocation>(b =>
            {
                b.ToTable("CollectionAllocations");
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Notes).HasMaxLength(300);

                b.HasIndex(x => new { x.CollectionId, x.Kind });

                b.HasOne<Collection>().WithMany(c => c.Allocations).HasForeignKey(x => x.CollectionId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne<Contract>().WithMany().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<Shop>().WithMany().HasForeignKey(x => x.ShopId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<MeterReading>().WithMany().HasForeignKey(x => x.MeterReadingId).OnDelete(DeleteBehavior.SetNull);
            });

            // === EK: Accrual (borç/tahakkuk) ===
            modelBuilder.Entity<Accrual>(b =>
            {
                b.ToTable("Accruals");
                b.HasKey(x => x.Id);

                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.OutstandingAmount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Notes).HasMaxLength(500);

                b.HasIndex(x => new { x.ShopId, x.TenantId, x.ContractId, x.Kind, x.Year, x.Month }).IsUnique();

                b.HasOne<Shop>().WithMany().HasForeignKey(x => x.ShopId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<Contract>().WithMany().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<MeterReading>().WithMany().HasForeignKey(x => x.MeterReadingId).OnDelete(DeleteBehavior.SetNull);
            });

            // === EK: AccrualSettlement (borç kapanış eşleşmesi) ===
            modelBuilder.Entity<AccrualSettlement>(b =>
            {
                b.ToTable("AccrualSettlements");
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Notes).HasMaxLength(300);

                b.HasIndex(x => new { x.AccrualId, x.CollectionAllocationId }).IsUnique();

                b.HasOne<Accrual>().WithMany().HasForeignKey(x => x.AccrualId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne<CollectionAllocation>().WithMany().HasForeignKey(x => x.CollectionAllocationId).OnDelete(DeleteBehavior.Cascade);
            });

            // === EK: ReserveAccount & ReserveEntry ===
            modelBuilder.Entity<ReserveAccount>(b =>
            {
                b.ToTable("ReserveAccounts");
                b.HasKey(x => x.Id);
                b.Property(x => x.DisplayName).HasMaxLength(200);
                b.HasIndex(x => x.Type).IsUnique(); // her tipten tek hesap (istersen kaldır)
            });

            modelBuilder.Entity<ReserveEntry>(b =>
            {
                b.ToTable("ReserveEntries");
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Description).HasMaxLength(500);
                b.Property(x => x.ExternalDocNo).HasMaxLength(100);

                b.HasIndex(x => new { x.ReserveAccountId, x.OccurredAtUtc });

                // ANA ilişki: Cascade kalabilir
                b.HasOne(x => x.ReserveAccount)
                 .WithMany() // karşı tarafta koleksiyon nav yoksa böyle kalır
                 .HasForeignKey(x => x.ReserveAccountId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Opsiyonel referanslar: Restrict/NoAction önerilir (SET NULL da olur ama çoğu zaman path’leri şişirir)
                b.HasOne(x => x.Collection)
                 .WithMany()
                 .HasForeignKey(x => x.CollectionId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.CollectionAllocation)
                 .WithMany()
                 .HasForeignKey(x => x.CollectionAllocationId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.MeterReading)
                 .WithMany()
                 .HasForeignKey(x => x.MeterReadingId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Contract)
                 .WithMany()
                 .HasForeignKey(x => x.ContractId)
                 .OnDelete(DeleteBehavior.Restrict);
            });


            // === EK: OwnerDistribution & Line ===
            modelBuilder.Entity<OwnerDistribution>(b =>
            {
                b.ToTable("OwnerDistributions");
                b.HasKey(x => x.Id);

                b.Property(x => x.OwnerAmountInput).HasColumnType("decimal(18,2)");
                b.Property(x => x.RateTax).HasColumnType("decimal(5,2)");
                b.Property(x => x.RateZakat).HasColumnType("decimal(5,2)");
                b.Property(x => x.RateCharity).HasColumnType("decimal(5,2)");
                b.Property(x => x.RateOwnerReserve).HasColumnType("decimal(5,2)");
                b.Property(x => x.RateFamilyPool).HasColumnType("decimal(5,2)");
                b.Property(x => x.AmountTax).HasColumnType("decimal(18,2)");
                b.Property(x => x.AmountZakat).HasColumnType("decimal(18,2)");
                b.Property(x => x.AmountCharity).HasColumnType("decimal(18,2)");
                b.Property(x => x.AmountOwnerReserve).HasColumnType("decimal(18,2)");
                b.Property(x => x.AmountFamilyPool).HasColumnType("decimal(18,2)");
                b.Property(x => x.Notes).HasMaxLength(500);
            });

            modelBuilder.Entity<OwnerDistributionLine>(b =>
            {
                b.ToTable("OwnerDistributionLines");
                b.HasKey(x => x.Id);
                b.Property(x => x.RateUsed).HasColumnType("decimal(5,2)");
                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Notes).HasMaxLength(300);

                b.HasOne<OwnerDistribution>().WithMany().HasForeignKey(x => x.OwnerDistributionId).OnDelete(DeleteBehavior.Cascade);
            });

            // === EK: FamilyMember ===
            modelBuilder.Entity<FamilyMember>(b =>
            {
                b.ToTable("FamilyMembers");
                b.HasKey(x => x.Id);
                b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
                b.Property(x => x.RelationTag).HasMaxLength(50);
            });

            // === EK: FamilyPayout & Line ===
            modelBuilder.Entity<FamilyPayout>(b =>
            {
                b.ToTable("FamilyPayouts");
                b.HasKey(x => x.Id);
                b.Property(x => x.TotalFamilyAmount).HasColumnType("decimal(18,2)");
                b.Property(x => x.RuleSummary).HasMaxLength(500);
            });

            modelBuilder.Entity<FamilyPayoutLine>(b =>
            {
                b.ToTable("FamilyPayoutLines");
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Notes).HasMaxLength(300);

                b.HasOne<FamilyPayout>().WithMany(p => p.Lines).HasForeignKey(x => x.FamilyPayoutId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne<FamilyMember>().WithMany().HasForeignKey(x => x.FamilyMemberId).OnDelete(DeleteBehavior.Restrict);
            });

            // === EK: FranchiseBalanceEntry ===
            modelBuilder.Entity<FranchiseBalanceEntry>(b =>
            {
                b.ToTable("FranchiseBalanceEntries");
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Notes).HasMaxLength(300);

                b.HasIndex(x => new { x.FranchiseOwnerId, x.OccurredAtUtc });

                b.HasOne<FranchiseOwner>().WithMany().HasForeignKey(x => x.FranchiseOwnerId).OnDelete(DeleteBehavior.Restrict);
            });

            // === EK: FamilyBalanceEntry ===
            modelBuilder.Entity<FamilyBalanceEntry>(b =>
            {
                b.ToTable("FamilyBalanceEntries");
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.Notes).HasMaxLength(300);

                b.HasIndex(x => new { x.FamilyMemberId, x.OccurredAtUtc });

                b.HasOne<FamilyMember>().WithMany().HasForeignKey(x => x.FamilyMemberId).OnDelete(DeleteBehavior.Restrict);
            });

            // === EK: Address (polimorfik) ===
            modelBuilder.Entity<Address>(b =>
            {
                b.ToTable("Addresses");
                b.HasKey(x => x.Id);

                b.Property(x => x.Neighborhood).HasMaxLength(150);
                b.Property(x => x.Street).HasMaxLength(150);
                b.Property(x => x.BuildingNo).HasMaxLength(50);
                b.Property(x => x.UnitNo).HasMaxLength(50);
                b.Property(x => x.PostalCode).HasMaxLength(20);
                b.Property(x => x.Notes).HasMaxLength(500);

                b.HasIndex(x => new { x.OwnerType, x.OwnerId, x.IsPrimary });

                b.HasOne(x => x.Province)
                    .WithMany(p => p.Addresses)               // if you have this collection; otherwise .WithMany()
                    .HasForeignKey(x => x.ProvinceId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.District)
                    .WithMany(d => d.Addresses)
                    .HasForeignKey(x => x.DistrictId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MarketMember>(b =>
            {
                b.ToTable("MarketMembers");
                b.HasKey(x => x.Id);
                b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
                b.Property(x => x.MemberType).HasMaxLength(50);
                b.Property(x => x.IsActive).HasDefaultValue(true);
            });

            // Tek oranlı politikalar için çakışmasın:
            modelBuilder.Entity<SingleRatePolicy>()
                .HasIndex(x => new { x.Type, x.EffectiveFrom });

            // OwnerDistributionPolicy & FamilySharePolicy için de benzer tarih indeksleri:
            modelBuilder.Entity<OwnerDistributionPolicy>()
                .HasIndex(x => x.EffectiveFrom);

            modelBuilder.Entity<FamilySharePolicy>()
                .HasIndex(x => x.EffectiveFrom);
        }
    }

}
