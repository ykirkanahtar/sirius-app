using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sirius.HousingPaymentPlans.Dto;

namespace Sirius.HousingPaymentPlans
{
    public interface IHousingPaymentPlanAppService : IAsyncCrudAppService<HousingPaymentPlanDto, Guid, PagedHousingPaymentPlanResultRequestDto, CreateCreditHousingPaymentPlanDto, UpdateHousingPaymentPlanDto>
    {
        Task CreateTransferForHousingDueAsync(CreateTransferForHousingDueDto input);
        Task CreateDebtPaymentForHousingCategoryAsync(CreateDebtHousingPaymentPlanForHousingCategoryDto input);
        Task<HousingPaymentPlanDto> CreateDebtPaymentAsync(CreateDebtHousingPaymentPlanDto input);
        Task<HousingPaymentPlanDto> CreateCreditPaymentAsync(CreateCreditHousingPaymentPlanDto input);
        Task<PagedResultDto<HousingPaymentPlanDto>> GetAllByHousingIdAsync(
            PagedHousingPaymentPlanResultRequestDto input);
    }
}
