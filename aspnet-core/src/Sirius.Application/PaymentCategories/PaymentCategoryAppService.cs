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
using Abp.Localization;
using Abp.Localization.Sources;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.PaymentCategories.Dto;
using Sirius.People.Dto;
using Sirius.Shared.Constants;
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
        private readonly ILocalizationSource _localizationSource;
        public PaymentCategoryAppService(
            IPaymentCategoryManager paymentCategoryManager,
            IRepository<PaymentCategory, Guid> paymentCategoryRepository
            , ILocalizationManager localizationManager)
            : base(paymentCategoryRepository)
        {
            _paymentCategoryManager = paymentCategoryManager;
            _paymentCategoryRepository = paymentCategoryRepository;
            _localizationSource = localizationManager.GetSource(AppConstants.LocalizationSourceName);
        }

        public override async Task<PaymentCategoryDto> CreateAsync(CreatePaymentCategoryDto input)
        {
            CheckCreatePermission();
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
            CheckUpdatePermission();
            var existingPaymentCategory = await _paymentCategoryManager.GetAsync(input.Id);
            var paymentCategory = PaymentCategory.Update(existingPaymentCategory, input.PaymentCategoryName);
            await _paymentCategoryManager.UpdateAsync(paymentCategory);
            return ObjectMapper.Map<PaymentCategoryDto>(paymentCategory);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var paymentCategory = await _paymentCategoryManager.GetAsync(input.Id);
            await _paymentCategoryManager.DeleteAsync(paymentCategory);
        }

        public override async Task<PagedResultDto<PaymentCategoryDto>> GetAllAsync(
            PagedPaymentCategoryResultRequestDto input)
        {
            CheckGetAllPermission();

            var query = _paymentCategoryRepository
                .GetAll()
                .WhereIf(!string.IsNullOrWhiteSpace(input.PaymentCategoryName),
                    p => p.PaymentCategoryName.Contains(input.PaymentCategoryName));

            var paymentCategories = await query
                .OrderBy(input.Sorting ?? $"{nameof(PaymentCategoryDto.PaymentCategoryName)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<PaymentCategoryDto>(query.Count(),
                ObjectMapper.Map<List<PaymentCategoryDto>>(paymentCategories));
        }

        public async Task<List<LookUpDto>> GetPaymentCategoryLookUpAsync()
        {
            CheckGetAllPermission();

            var paymentAccounts = await _paymentCategoryRepository.GetAllListAsync(p => p.ShowInLists);

            return
                (from l in paymentAccounts
                    select new LookUpDto(l.Id.ToString(), _localizationSource.GetString(l.PaymentCategoryName))).ToList();
        }

        public async Task<List<string>> GetPaymentCategoryFromAutoCompleteFilterAsync(string request)
        {
            CheckGetAllPermission();

            var query = from p in _paymentCategoryRepository.GetAll()
                where p.PaymentCategoryName.Contains(request) && p.ShowInLists
                select _localizationSource.GetString(p.PaymentCategoryName);

            return await query.ToListAsync();
        }
    }
}