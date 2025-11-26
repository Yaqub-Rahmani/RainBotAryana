using Autofac;
using Autofac.Extras.DynamicProxy;
using Business.Abstract;
using Business.Concrete;
using Business.Security.Jwt;
using Castle.DynamicProxy;
using Core.Utilities.Files;
using Core.Utilities.Interceptors;
using Core.Utilities.KabulDateTime;
using Core.Utilities.Security.Jwt;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.DependencyResolvers.Autofac
{
    public class AutofacBusinessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterType<FingerprintManager>().As<IFingerprintService>().InstancePerLifetimeScope();

            builder.RegisterType<KabulTimeProvider>()
                   .As<ITimeProvider>()
                   .SingleInstance();

            // 🔹 Dosya yolu servisi (IPathProvider'a bağımlı)
            builder.RegisterType<FilePathManager>()
                   .As<IFilePathService>()
                   .SingleInstance();

            // 🔹 Görsel kaydetme servisi
            builder.RegisterType<ImageStorageManager>()
                   .As<IImageStorageService>()
                   .InstancePerLifetimeScope();

            // 🔹 Genel dosya yükleme (FormData) servisi
            builder.RegisterType<FileStorageManager>()
                   .As<IFileStorageService>()
                   .InstancePerLifetimeScope();


            builder.RegisterType<EfFranchiseOwnerBalanceDal>().As<IFranchiseOwnerBalanceDal>().InstancePerLifetimeScope();
            builder.RegisterType<FranchiseOwnerBalanceManager>().As<IFranchiseOwnerBalanceService>().InstancePerLifetimeScope();

            builder.RegisterType<EfAccrualDal>().As<IAccrualDal>().InstancePerLifetimeScope();
            builder.RegisterType<AccrualManager>().As<IAccrualService>().InstancePerLifetimeScope();

            builder.RegisterType<EfAccrualSettlementDal>().As<IAccrualSettlementDal>().InstancePerLifetimeScope();
            builder.RegisterType<AccrualSettlementManager>().As<IAccrualSettlementService>().InstancePerLifetimeScope();



            builder.RegisterType<EfAddressDal>().As<IAddressDal>().InstancePerLifetimeScope();
            builder.RegisterType<AddressManager>().As<IAddressService>().InstancePerLifetimeScope();

            // Collections
            builder.RegisterType<EfCollectionDal>().As<ICollectionDal>().InstancePerLifetimeScope();
            builder.RegisterType<CollectionManager>().As<ICollectionService>().InstancePerLifetimeScope();

            builder.RegisterType<EfCollectionAllocationDal>().As<ICollectionAllocationDal>().InstancePerLifetimeScope();
            builder.RegisterType<CollectionAllocationManager>().As<ICollectionAllocationService>().InstancePerLifetimeScope();

            builder.RegisterType<EfContractDal>().As<IContractDal>().InstancePerLifetimeScope();
            builder.RegisterType<ContractManager>().As<IContractService>().InstancePerLifetimeScope();

            builder.RegisterType<EfDistrictDal>().As<IDistrictDal>().InstancePerLifetimeScope();
            builder.RegisterType<DistrictManager>().As<IDistrictService>().InstancePerLifetimeScope();

            builder.RegisterType<EfFamilyBalanceEntryDal>().As<IFamilyBalanceEntryDal>().InstancePerLifetimeScope();
            builder.RegisterType<FamilyBalanceEntryManager>().As<IFamilyBalanceEntryService>().InstancePerLifetimeScope();

            builder.RegisterType<EfFamilyMemberDal>().As<IFamilyMemberDal>().InstancePerLifetimeScope();
            builder.RegisterType<FamilyMemberManager>().As<IFamilyMemberService>().InstancePerLifetimeScope();

            builder.RegisterType<EfFamilyPayoutDal>().As<IFamilyPayoutDal>().InstancePerLifetimeScope();
            builder.RegisterType<FamilyPayoutManager>().As<IFamilyPayoutService>().InstancePerLifetimeScope();

            builder.RegisterType<EfFamilyPayoutLineDal>().As<IFamilyPayoutLineDal>().InstancePerLifetimeScope();
            builder.RegisterType<FamilyPayoutLineManager>().As<IFamilyPayoutLineService>().InstancePerLifetimeScope();

            builder.RegisterType<EfFamilySharePolicyDal>().As<IFamilySharePolicyDal>().InstancePerLifetimeScope();
            builder.RegisterType<FamilySharePolicyManager>().As<IFamilySharePolicyService>().InstancePerLifetimeScope();

            builder.RegisterType<EfFloorDal>().As<IFloorDal>().InstancePerLifetimeScope();
            builder.RegisterType<FloorManager>().As<IFloorService>().InstancePerLifetimeScope();

            builder.RegisterType<EfFranchiseBalanceEntryDal>().As<IFranchiseBalanceEntryDal>().InstancePerLifetimeScope();
            builder.RegisterType<FranchiseBalanceEntryManager>().As<IFranchiseBalanceEntryService>().InstancePerLifetimeScope();

            builder.RegisterType<EfFranchiseOwnerDal>().As<IFranchiseOwnerDal>().InstancePerLifetimeScope();
            builder.RegisterType<FranchiseOwnerManager>().As<IFranchiseOwnerService>().InstancePerLifetimeScope();

            builder.RegisterType<EfMarketDal>().As<IMarketDal>().InstancePerLifetimeScope();
            builder.RegisterType<MarketManager>().As<IMarketService>().InstancePerLifetimeScope();

            builder.RegisterType<EfMarketMemberDal>().As<IMarketMemberDal>().InstancePerLifetimeScope();
            builder.RegisterType<MarketMemberManager>().As<IMarketMemberService>().InstancePerLifetimeScope();

            builder.RegisterType<EfMeterReadingDal>().As<IMeterReadingDal>().InstancePerLifetimeScope();
            builder.RegisterType<MeterReadingManager>().As<IMeterReadingService>().InstancePerLifetimeScope();

            builder.RegisterType<EfOwnerDistributionDal>().As<IOwnerDistributionDal>().InstancePerLifetimeScope();
            builder.RegisterType<OwnerDistributionManager>().As<IOwnerDistributionService>().InstancePerLifetimeScope();

            builder.RegisterType<EfOwnerDistributionLineDal>().As<IOwnerDistributionLineDal>().InstancePerLifetimeScope();
            builder.RegisterType<OwnerDistributionLineManager>().As<IOwnerDistributionLineService>().InstancePerLifetimeScope();

            builder.RegisterType<EfOwnerDistributionPolicyDal>().As<IOwnerDistributionPolicyDal>().InstancePerLifetimeScope();
            builder.RegisterType<OwnerDistributionPolicyManager>().As<IOwnerDistributionPolicyService>().InstancePerLifetimeScope();

            builder.RegisterType<EfProvinceDal>().As<IProvinceDal>().InstancePerLifetimeScope();
            builder.RegisterType<ProvinceManager>().As<IProvinceService>().InstancePerLifetimeScope();

            builder.RegisterType<EfReserveAccountDal>().As<IReserveAccountDal>().InstancePerLifetimeScope();
            builder.RegisterType<ReserveAccountManager>().As<IReserveAccountService>().InstancePerLifetimeScope();

            builder.RegisterType<EfReserveEntryDal>().As<IReserveEntryDal>().InstancePerLifetimeScope();
            builder.RegisterType<ReserveEntryManager>().As<IReserveEntryService>().InstancePerLifetimeScope();

            builder.RegisterType<EfShopDal>().As<IShopDal>().InstancePerLifetimeScope();
            builder.RegisterType<ShopManager>().As<IShopService>().InstancePerLifetimeScope();

            builder.RegisterType<EfShopFranchiseShareDal>().As<IShopFranchiseShareDal>().InstancePerLifetimeScope();
            builder.RegisterType<ShopFranchiseShareManager>().As<IShopFranchiseShareService>().InstancePerLifetimeScope();

            builder.RegisterType<EfSingleRatePolicyDal>().As<ISingleRatePolicyDal>().InstancePerLifetimeScope();
            builder.RegisterType<SingleRatePolicyManager>().As<ISingleRatePolicyService>().InstancePerLifetimeScope();

            builder.RegisterType<EfTenantDal>().As<ITenantDal>().InstancePerLifetimeScope();
            builder.RegisterType<TenantManager>().As<ITenantService>().InstancePerLifetimeScope();

            builder.RegisterType<EfBiometricDal>().As<IBiometricDal>().InstancePerLifetimeScope();
            builder.RegisterType<BiometricManager>().As<IBiometricService>().InstancePerLifetimeScope();

            builder.RegisterType<EfPhoneDal>().As<IPhoneDal>().InstancePerLifetimeScope();
            builder.RegisterType<PhoneManager>().As<IPhoneService>().InstancePerLifetimeScope();

            builder.RegisterType<EfMediaAssetDal>().As<IMediaAssetDal>().InstancePerLifetimeScope();
            builder.RegisterType<MediaAssetManager>().As<IMediaAssetService>().InstancePerLifetimeScope();

            builder.RegisterType<TaxPaymentEntryManager>().As<ITaxPaymentEntryService>().InstancePerLifetimeScope();
            builder.RegisterType<EfTaxPaymentEntryDal>().As<ITaxPaymentEntryDal>().InstancePerLifetimeScope();

            // ZakatPaymentEntry
            builder.RegisterType<ZakatPaymentEntryManager>().As<IZakatPaymentEntryService>().InstancePerLifetimeScope();
            builder.RegisterType<EfZakatPaymentEntryDal>().As<IZakatPaymentEntryDal>().InstancePerLifetimeScope();

            // CharityPaymentEntry
            builder.RegisterType<CharityPaymentEntryManager>().As<ICharityPaymentEntryService>().InstancePerLifetimeScope();
            builder.RegisterType<EfCharityPaymentEntryDal>().As<ICharityPaymentEntryDal>().InstancePerLifetimeScope();







            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            builder.RegisterType<UserManager>().As<IUserService>().InstancePerLifetimeScope();
            builder.RegisterType<EfUserDal>().As<IUserDal>().InstancePerLifetimeScope();

            builder.RegisterType<AuthManager>().As<IAuthService>().InstancePerLifetimeScope();
            builder.RegisterType<JwtHelper>().SingleInstance();

            // AOP
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
              .EnableInterfaceInterceptors(new ProxyGenerationOptions { Selector = new AspectInterceptorSelector() })
              .InstancePerLifetimeScope();
        }
    }
}
