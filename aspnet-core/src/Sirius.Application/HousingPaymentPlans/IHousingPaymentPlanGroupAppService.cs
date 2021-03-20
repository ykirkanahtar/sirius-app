using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.HousingPaymentPlans.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.HousingPaymentPlans
{
    public interface IHousingPaymentPlanGroupAppService : IAsyncCrudAppService<HousingPaymentPlanGroupDto, Guid,
        PagedHousingPaymentPlanGroupResultRequestDto, CreateHousingPaymentPlanGroupDto, UpdateHousingPaymentPlanGroupDto
    >
    {
        Task<UpdateHousingPaymentPlanGroupDto> GetForUpdate(Guid id);
        List<LookUpDto> GetResidentOrOwnerLookUp();
    }
}