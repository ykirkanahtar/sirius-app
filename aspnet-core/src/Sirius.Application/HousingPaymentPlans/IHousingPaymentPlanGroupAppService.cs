using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.HousingPaymentPlans.Dto;

namespace Sirius.HousingPaymentPlans
{
    public interface IHousingPaymentPlanGroupAppService : IAsyncCrudAppService<HousingPaymentPlanGroupDto, Guid,
        PagedHousingPaymentPlanGroupResultRequestDto, CreateHousingPaymentPlanGroupDto, UpdateHousingPaymentPlanGroupDto
    >
    {
        Task<UpdateHousingPaymentPlanGroupDto> GetForUpdate(Guid id);
    }
}