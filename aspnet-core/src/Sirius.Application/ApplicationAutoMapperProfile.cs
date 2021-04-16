using Abp.AutoMapper;
using Abp.Modules;
using Sirius.PaymentAccounts.Dto;

namespace Sirius
{
    [DependsOn(typeof(AbpAutoMapperModule))]
    public class ApplicationAutoMapperProfile : AutoMapper.Profile
    {
        public ApplicationAutoMapperProfile()
        {
            CreateMap<AccountBookGetAllOutput, AccountBookGetAllExportOutput>()
                .ForMember(x => x.ProcessDateTime,
                    opt => opt.MapFrom(src =>
                        src.ProcessDateTime.ToString("dd.MM.yyyy")));

            // $"{src.ProcessDateTime.Day:00}.{src.ProcessDateTime.Month:0}.{src.ProcessDateTime.Year}"));
        }
    }
}