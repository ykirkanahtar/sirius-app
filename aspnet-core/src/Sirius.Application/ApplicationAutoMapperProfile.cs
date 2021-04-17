using Abp.AutoMapper;
using Abp.Dependency;
using Abp.Localization;
using Abp.Modules;
using Sirius.HousingPaymentPlans;
using Sirius.HousingPaymentPlans.Dto;
using Sirius.PaymentAccounts.Dto;
using Sirius.Shared.Constants;

namespace Sirius
{
    [DependsOn(typeof(AbpAutoMapperModule))]
    public class ApplicationAutoMapperProfile : AutoMapper.Profile
    {

        public ApplicationAutoMapperProfile()
        {
            var iocManager = IocManager.Instance;
            var localizationManager = iocManager.Resolve<ILocalizationManager>();
            var localizationSource = localizationManager.GetSource(AppConstants.LocalizationSourceName);
            
            CreateMap<AccountBookGetAllOutput, AccountBookGetAllExportOutput>()
                .ForMember(x => x.ProcessDateTime,
                    opt => opt.MapFrom(src =>
                        src.ProcessDateTime.ToString("dd.MM.yyyy")));

            CreateMap<HousingPaymentPlan, HousingPaymentPlanExportOutput>()
                .ForMember(x => x.Date,
                    opt => opt.MapFrom(src => src.Date.ToString("dd.MM.yyyy")))
                .ForMember(x => x.PaymentCategory,
                    opt => opt.MapFrom(src => src.PaymentCategory.PaymentCategoryName))
                .ForMember(x => x.CreditOrDebt,
                    opt => opt.MapFrom(src => localizationSource.GetString(src.CreditOrDebt.ToString())))
                .ForMember(x => x.HousingPaymentPlanType,
                    opt => opt.MapFrom(src => localizationSource.GetString(src.HousingPaymentPlanType.ToString())));
        }
    }
}