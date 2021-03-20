using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.Authorization;
using Sirius.EntityFrameworkCore.Repositories;
using Sirius.HousingPaymentPlans;
using Sirius.Housings.Dto;
using Sirius.PaymentCategories;
using Sirius.People;
using Sirius.People.Dto;
using Sirius.Shared.Dtos;
using Sirius.Shared.Enums;

namespace Sirius.Housings
{
    [AbpAuthorize(PermissionNames.Pages_Housings)]
    public class HousingAppService :
        AsyncCrudAppService<Housing, HousingDto, Guid, PagedHousingResultRequestDto, CreateHousingDto, UpdateHousingDto
        >, IHousingAppService
    {
        private readonly IHousingManager _housingManager;
        private readonly IHousingRepository _housingRepository;
        private readonly IRepository<HousingCategory, Guid> _housingCategoryRepository;
        private readonly IPersonManager _personManager;
        private readonly IRepository<Person, Guid> _personRepository;
        private readonly IRepository<HousingPerson, Guid> _housingPersonRepository;
        private readonly IRepository<Block, Guid> _blockRepository;
        private readonly IHousingPolicy _housingPolicy;
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;

        public HousingAppService(IHousingManager housingManager, IHousingRepository housingRepository,
            IRepository<HousingCategory, Guid> housingCategoryRepository, IPersonManager personManager,
            IRepository<Person, Guid> personRepository, IRepository<HousingPerson, Guid> housingPersonRepository,
            IRepository<Block, Guid> blockRepository, IHousingPolicy housingPolicy,
            IPaymentCategoryManager paymentCategoryManager, IHousingPaymentPlanManager housingPaymentPlanManager)
            : base(housingRepository)
        {
            _housingManager = housingManager;
            _housingRepository = housingRepository;
            _housingCategoryRepository = housingCategoryRepository;
            _personManager = personManager;
            _personRepository = personRepository;
            _housingPersonRepository = housingPersonRepository;
            _blockRepository = blockRepository;
            _housingPolicy = housingPolicy;
            _paymentCategoryManager = paymentCategoryManager;
            _housingPaymentPlanManager = housingPaymentPlanManager;
        }

        public override async Task<HousingDto> CreateAsync(CreateHousingDto input)
        {
            try
            {
                CheckCreatePermission();
                var housingCategory = await _housingCategoryRepository.GetAsync(input.HousingCategoryId);
                var block = await _blockRepository.GetAsync(input.BlockId);

                var housing = await Housing.CreateAsync(_housingPolicy, SequentialGuidGenerator.Instance.Create(),
                    AbpSession.GetTenantId(),
                    block, input.Apartment, housingCategory, input.TenantIsResiding);
                
                if (input.CreateTransferForHousingDue.Amount != 0)
                {
                    housing = input.CreateTransferForHousingDue.IsDebt
                        ? Housing.IncreaseBalance(housing, input.CreateTransferForHousingDue.Amount, input.CreateTransferForHousingDue.ResidentOrOwner)
                        : Housing.DecreaseBalance(housing, input.CreateTransferForHousingDue.Amount, input.CreateTransferForHousingDue.ResidentOrOwner);
                }

                await _housingManager.CreateAsync(housing);

                if (input.CreateTransferForHousingDue.Amount != 0)
                {
                    // var paymentCategory = await _paymentCategoryManager.GetTransferForRegularHousingDueAsync();
                    // var paymentCategory = await _paymentCategoryManager.GetAsync(input.CreateTransferForHousingDue.PaymentCategoryId);

                    var housingPaymentPlan = input.CreateTransferForHousingDue.IsDebt
                        ? HousingPaymentPlan.CreateDebt(
                            SequentialGuidGenerator.Instance.Create()
                            , AbpSession.GetTenantId()
                            , null
                            , housing
                            , null
                            , input.CreateTransferForHousingDue.Date
                            , input.CreateTransferForHousingDue.Amount
                            , input.CreateTransferForHousingDue.Description
                            , HousingPaymentPlanType.Transfer
                            , null
                        )
                        : HousingPaymentPlan.CreateCredit(
                            SequentialGuidGenerator.Instance.Create()
                            , AbpSession.GetTenantId()
                            , housing
                            , null
                            , input.CreateTransferForHousingDue.Date
                            , input.CreateTransferForHousingDue.Amount
                            , input.CreateTransferForHousingDue.Description
                            , null
                            , HousingPaymentPlanType.Transfer
                            , null
                        );

                    await _housingPaymentPlanManager.CreateAsync(housingPaymentPlan);
                }

                return ObjectMapper.Map<HousingDto>(housing);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public override async Task<HousingDto> UpdateAsync(UpdateHousingDto input)
        {
            CheckUpdatePermission();
            var existingHousing = await _housingManager.GetAsync(input.Id);
            var oldTenantIsResidingValue = existingHousing.TenantIsResiding;

            var housingCategory = await _housingCategoryRepository.GetAsync(input.HousingCategoryId);
            var block = await _blockRepository.GetAsync(input.BlockId);

            var housing = await Housing.UpdateAsync(_housingPolicy, existingHousing, block, input.Apartment,
                housingCategory, input.TenantIsResiding);
            await _housingManager.UpdateAsync(housing);

            //Eğer kiralık seçeneği kaldırılırsa, kiracı olarak işaretlenen kişiler de siliniyor
            if (oldTenantIsResidingValue && housing.TenantIsResiding == false)
            {
                var tenantPeople =
                    await _housingPersonRepository.GetAllListAsync(p =>
                        p.HousingId == existingHousing.Id && p.IsTenant);

                tenantPeople.ForEach(async x => await _housingPersonRepository.DeleteAsync(x.Id));
            }

            return ObjectMapper.Map<HousingDto>(housing);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var housing = await _housingManager.GetAsync(input.Id);
            await _housingManager.DeleteAsync(housing);
        }

        public async Task<PagedResultDto<HousingForListDto>> GetAllListAsync(PagedHousingResultRequestDto input)
        {
            CheckGetAllPermission();
            var query = (from housing in _housingRepository.GetAll().Include(p => p.HousingCategory)
                        .Include(p => p.Block)
                    join housingPerson in _housingPersonRepository.GetAll() on housing.Id equals housingPerson
                            .HousingId
                        into g1
                    from housingPerson in g1.DefaultIfEmpty()
                    join person in _personRepository.GetAll() on housingPerson.PersonId equals person.Id into g2
                    from person in g2.DefaultIfEmpty()
                    select new {housing, housingPerson, person})
                .WhereIf(input.HousingIds.Count > 0, p => input.HousingIds.Contains(p.housing.Id))
                .WhereIf(input.HousingCategoryIds.Count > 0,
                    p => input.HousingCategoryIds.Contains(p.housing.HousingCategoryId))
                .WhereIf(input.PersonIds.Count > 0,
                    p => input.PersonIds.Contains(p.person != null ? p.person.Id : Guid.Empty))
                .GroupBy(p => new HousingForListDto
                {
                    Id = p.housing.Id,
                    Apartment = p.housing.Apartment,
                    Block = p.housing.Block.BlockName,
                    HousingCategoryName = p.housing.HousingCategory.HousingCategoryName,
                    TenantIsResiding = p.housing.TenantIsResiding,
                    Balance = p.housing.Balance,
                    ResidentBalance = p.housing.ResidentBalance,
                    OwnerBalance = p.housing.OwnerBalance
                })
                .Select(p => p.Key);

            var housings = await query
                .OrderBy(input.Sorting ?? $"{nameof(HousingDto.Block)} ASC, {nameof(HousingDto.Apartment)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<HousingForListDto>(query.Count(), housings);
        }

        public async Task<List<LookUpDto>> GetHousingLookUpAsync()
        {
            CheckGetAllPermission();
            var query = _housingRepository.GetAll().Include(p => p.Block);

            return
                (from l in await query.ToListAsync()
                    select new LookUpDto(l.Id.ToString(), l.GetName())).ToList();
        }

        public async Task<HousingPersonDto> AddPersonAsync(CreateHousingPersonDto input)
        {
            CheckUpdatePermission();
            var housing = await _housingManager.GetAsync(input.HousingId);
            var person = await _personManager.GetAsync(input.PersonId);

            var housingPerson = await _housingManager.AddPersonAsync(housing, person, input.IsTenant, input.Contact);
            return ObjectMapper.Map<HousingPersonDto>(housingPerson);
        }

        public async Task RemovePersonAsync(RemoveHousingPersonDto input)
        {
            CheckUpdatePermission();
            var housing = await _housingManager.GetAsync(input.HousingId);
            var person = await _personManager.GetAsync(input.PersonId);

            var housingPerson = await _housingPersonRepository.GetAll()
                .Where(p => p.HousingId == input.HousingId && p.PersonId == input.PersonId).SingleAsync();
            await _housingPersonRepository.DeleteAsync(housingPerson);
        }

        public async Task<List<LookUpDto>> GetPeopleLookUpAsync(Guid housingId)
        {
            CheckGetAllPermission();
            var housing = await _housingRepository.GetAsync(housingId);

            var housingPeople = await _housingPersonRepository.GetAllListAsync(p => p.HousingId == housingId);

            var peopleQuery = _personRepository.GetAll();
            var people = await peopleQuery.WhereIf(housingPeople.Count > 0,
                p => housingPeople.Select(x => x.PersonId).Contains(p.Id) == false).ToListAsync();


            if (people.Count == 0)
            {
                throw new UserFriendlyException("Sistemde uygun kişi bulunamadı.");
            }

            return people.Select(p => new LookUpDto(p.Id.ToString(), $"{p.FirstName} {p.LastName}")).ToList();
        }

        public async Task<PagedResultDto<HousingPersonDto>> GetHousingPeopleAsync(
            PagedHousingPersonResultRequestDto input)
        {
            CheckGetAllPermission();
            await _housingRepository.GetAsync(input.HousingId);
            var query = _housingPersonRepository.GetAll().Include(p => p.Person)
                .Where(p => p.HousingId == input.HousingId);

            var housingPeople = await query
                .OrderBy(input.Sorting ??
                         $"{nameof(Person)}.{nameof(PersonDto.FirstName)} ASC, {nameof(Person)}.{nameof(PersonDto.LastName)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<HousingPersonDto>(query.Count(),
                ObjectMapper.Map<List<HousingPersonDto>>(housingPeople));
        }

        public async Task<List<LookUpDto>> GetHousingsLookUpByPersonIdAsync(Guid personId)
        {
            var housings = await (from h in _housingRepository.GetAll().Include(p => p.Block)
                join hp in _housingPersonRepository.GetAll() on h.Id equals hp.HousingId
                where hp.PersonId == personId
                select h).ToListAsync();

            if (housings.Count == 0)
            {
                throw new UserFriendlyException("Sistemde uygun konut bulunamadı.");
            }

            return housings.Select(p => new LookUpDto(p.Id.ToString(), p.GetName())).ToList();
        }
    }
}