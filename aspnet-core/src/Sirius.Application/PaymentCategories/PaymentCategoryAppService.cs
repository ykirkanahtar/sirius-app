using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Sirius.PaymentCategories.Dto;
using Sirius.Shared.Dtos;
using Sirius.Shared.Enums;

namespace Sirius.PaymentCategories
{
    public class PaymentCategoryAppService :
        AsyncCrudAppService<PaymentCategory, PaymentCategoryDto, Guid, PagedPaymentCategoryResultRequestDto,
            CreatePaymentCategoryDto, UpdatePaymentCategoryDto>, IPaymentCategoryAppService
    {
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;

        public PaymentCategoryAppService(IPaymentCategoryManager paymentCategoryManager,
            IRepository<PaymentCategory, Guid> paymentCategoryRepository)
            : base(paymentCategoryRepository)
        {
            _paymentCategoryManager = paymentCategoryManager;
            _paymentCategoryRepository = paymentCategoryRepository;
        }

        public override async Task<PaymentCategoryDto> CreateAsync(CreatePaymentCategoryDto input)
        {
            if (input.HousingDueType == HousingDueType.RegularHousingDue)
            {
                throw new UserFriendlyException("Geçersiz aidat ödemesi tipi");
            }

            var paymentCategory = PaymentCategory.Create(SequentialGuidGenerator.Instance.Create(),
                AbpSession.GetTenantId(), input.PaymentCategoryName, input.HousingDueType, input.IsValidForAllPeriods);
            await _paymentCategoryManager.CreateAsync(paymentCategory);
            return ObjectMapper.Map<PaymentCategoryDto>(paymentCategory);
        }

        public override async Task<PaymentCategoryDto> UpdateAsync(UpdatePaymentCategoryDto input)
        {
            var existingPaymentCategory = await _paymentCategoryManager.GetAsync(input.Id);
            var paymentCategory = PaymentCategory.Update(existingPaymentCategory, input.PaymentCategoryName);
            await _paymentCategoryManager.UpdateAsync(paymentCategory);
            return ObjectMapper.Map<PaymentCategoryDto>(paymentCategory);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            var paymentCategory = await _paymentCategoryManager.GetAsync(input.Id);
            await _paymentCategoryManager.DeleteAsync(paymentCategory);
        }

        public override async Task<PagedResultDto<PaymentCategoryDto>> GetAllAsync(
            PagedPaymentCategoryResultRequestDto input)
        {
            var query = await _paymentCategoryManager.GetAllAsync(AbpSession.GetTenantId(), input);

            return new PagedResultDto<PaymentCategoryDto>(query.Count,
                ObjectMapper.Map<List<PaymentCategoryDto>>(query));
        }
        
        public async Task<List<LookUpDto>> GetPaymentCategoryLookUpAsync()
        {
            var paymentAccounts = await _paymentCategoryRepository.GetAllListAsync();         
                                                                               
            return                                                             
                (from l in paymentAccounts                                            
                    select new LookUpDto(l.Id.ToString(), l.PaymentCategoryName)).ToList();  
        }
    }
}
