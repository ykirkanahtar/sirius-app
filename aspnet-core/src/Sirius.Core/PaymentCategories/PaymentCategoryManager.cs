using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.Shared.Enums;

namespace Sirius.PaymentCategories
{
    public class PaymentCategoryManager : IPaymentCategoryManager
    {
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private IPaymentCategoryManager _paymentCategoryManagerImplementation;

        public PaymentCategoryManager(IRepository<PaymentCategory, Guid> paymentCategoryRepository, IUnitOfWorkManager unitOfWorkManager)
        {
            _paymentCategoryRepository = paymentCategoryRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task CreateAsync(PaymentCategory paymentCategory)
        {
            await _paymentCategoryRepository.InsertAsync(paymentCategory);
        }

        public async Task UpdateAsync(PaymentCategory paymentCategory)
        {
            await _paymentCategoryRepository.UpdateAsync(paymentCategory);
        }
        
        public async Task DeleteAsync(PaymentCategory paymentCategory)
        {
            await _paymentCategoryRepository.DeleteAsync(paymentCategory);
        }

        public async Task<PaymentCategory> GetRegularHousingDueAsync()
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var query = await _paymentCategoryRepository.GetAllListAsync(p =>
                    p.HousingDueType == HousingDueType.RegularHousingDue);

                return query.Single();
            }
        }

        public async Task<PaymentCategory> GetAsync(Guid id)
        {
            var paymentCategory = await _paymentCategoryRepository.GetAsync(id);
            if(paymentCategory == null)
            {
                throw new UserFriendlyException("Kategori bulunamadı");
            }
            return paymentCategory;
        }
        
        public async Task<List<PaymentCategory>> GetAllAsync(int tenantId, PagedResultRequestDto pagingRequest)
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var query = _paymentCategoryRepository
                    .GetAll()
                    .Where(p => p.TenantId == null || p.TenantId == tenantId);

                if (pagingRequest != null)
                    query = query.PageBy(pagingRequest);

                return await query.ToListAsync();
            }
        }
    }
}
