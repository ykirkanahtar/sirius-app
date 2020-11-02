using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.PaymentCategories.Dto;
using Sirius.People.Dto;
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
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public PaymentCategoryAppService(IPaymentCategoryManager paymentCategoryManager,
            IRepository<PaymentCategory, Guid> paymentCategoryRepository, IUnitOfWorkManager unitOfWorkManager)
            : base(paymentCategoryRepository)
        {
            _paymentCategoryManager = paymentCategoryManager;
            _paymentCategoryRepository = paymentCategoryRepository;
            _unitOfWorkManager = unitOfWorkManager;
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
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var query = _paymentCategoryRepository
                    .GetAll()
                    .Where(p => p.TenantId == null || p.TenantId == AbpSession.TenantId)
                    .WhereIf(!string.IsNullOrWhiteSpace(input.PaymentCategoryName),
                        p => p.PaymentCategoryName.Contains(input.PaymentCategoryName));

                var paymentCategories = await query
                    .OrderBy(input.Sorting ?? $"{nameof(PaymentCategoryDto.PaymentCategoryName)}")
                    .PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<PaymentCategoryDto>(query.Count(),
                    ObjectMapper.Map<List<PaymentCategoryDto>>(paymentCategories));
            }
        }

        public async Task<List<LookUpDto>> GetPaymentCategoryLookUpAsync()
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var paymentAccounts = await _paymentCategoryRepository.GetAllListAsync();

                return
                    (from l in paymentAccounts
                        select new LookUpDto(l.Id.ToString(), l.PaymentCategoryName)).ToList();
            }
        }
        
        public async Task<List<string>> GetPaymentCategoryFromAutoCompleteFilterAsync(string request)
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var query = from p in _paymentCategoryRepository.GetAll()
                    where p.PaymentCategoryName.Contains(request)
                    select p.PaymentCategoryName;

                return await query.ToListAsync();
            }
        }
    }
}