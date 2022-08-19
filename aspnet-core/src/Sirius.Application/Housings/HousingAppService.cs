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
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Sirius.Authorization;
using Sirius.EntityFrameworkCore.Repositories;
using Sirius.HousingPaymentPlans;
using Sirius.HousingPaymentPlans.Dto;
using Sirius.Housings.Dto;
using Sirius.PaymentCategories;
using Sirius.People;
using Sirius.People.Dto;
using Sirius.Periods;
using Sirius.Shared.Dtos;
using Sirius.Shared.Enums;
using Sirius.Shared.Helper;

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
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IPeriodManager _periodManager;

        public HousingAppService(IHousingManager housingManager, IHousingRepository housingRepository,
            IRepository<HousingCategory, Guid> housingCategoryRepository, IPersonManager personManager,
            IRepository<Person, Guid> personRepository, IRepository<HousingPerson, Guid> housingPersonRepository,
            IRepository<Block, Guid> blockRepository, IHousingPolicy housingPolicy,
            IPaymentCategoryManager paymentCategoryManager, IHousingPaymentPlanManager housingPaymentPlanManager,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository, IPeriodManager periodManager)
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
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _periodManager = periodManager;
        }

        public override async Task<HousingDto> CreateAsync(CreateHousingDto input)
        {
            try
            {
                CheckCreatePermission();
                var housingCategory = await _housingCategoryRepository.GetAsync(input.HousingCategoryId);
                var block = await _blockRepository.GetAsync(input.BlockId);
                var activePeriod = await _periodManager.GetActivePeriod();

                var housing = await Housing.CreateAsync(_housingPolicy, SequentialGuidGenerator.Instance.Create(),
                    AbpSession.GetTenantId(),
                    block, input.Apartment, housingCategory, input.TenantIsResiding);

                if (input.TransferForHousingDue.Amount.GetValueOrDefault() != 0)
                {
                    housing = input.TransferForHousingDue.IsDebt
                        ? Housing.IncreaseBalance(housing, input.TransferForHousingDue.Amount.GetValueOrDefault(),
                            input.TransferForHousingDue.ResidentOrOwner)
                        : Housing.DecreaseBalance(housing, input.TransferForHousingDue.Amount.GetValueOrDefault(),
                            input.TransferForHousingDue.ResidentOrOwner);
                }

                await _housingManager.CreateAsync(housing);

                if (input.TransferForHousingDue.Amount.GetValueOrDefault() != 0)
                {
                    var housingPaymentPlan = input.TransferForHousingDue.IsDebt
                        ? HousingPaymentPlan.CreateDebt(
                            SequentialGuidGenerator.Instance.Create()
                            , AbpSession.GetTenantId()
                            , null
                            , housing
                            , input.TransferForHousingDue.ResidentOrOwner
                            , null
                            , input.TransferForHousingDue.TransferDateString.StringToDateTime()
                            , input.TransferForHousingDue.Amount.GetValueOrDefault()
                            , input.TransferForHousingDue.Description
                            , HousingPaymentPlanType.Transfer
                            , null
                            , input.TransferForHousingDue.ResidentOrOwner
                            , activePeriod.Id
                        )
                        : HousingPaymentPlan.CreateCredit(
                            SequentialGuidGenerator.Instance.Create()
                            , AbpSession.GetTenantId()
                            , housing
                            , input.TransferForHousingDue.ResidentOrOwner
                            , null
                            , input.TransferForHousingDue.TransferDateString.StringToDateTime()
                            , input.TransferForHousingDue.Amount.GetValueOrDefault()
                            , input.TransferForHousingDue.Description
                            , null
                            , HousingPaymentPlanType.Transfer
                            , null
                            , input.TransferForHousingDue.ResidentOrOwner
                            , activePeriod.Id
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

            var activePeriod = await _periodManager.GetActivePeriod();

            //Eğer kiralık seçeneği kaldırılırsa, kiracı olarak işaretlenen kişiler de siliniyor
            if (oldTenantIsResidingValue && housing.TenantIsResiding == false)
            {
                var tenantPeople =
                    await _housingPersonRepository.GetAllListAsync(p =>
                        p.HousingId == existingHousing.Id && p.IsTenant);

                tenantPeople.ForEach(async x => await _housingPersonRepository.DeleteAsync(x.Id));
            }

            /*Devir işlemleri*/
            if (input.TransferAmount.GetValueOrDefault() != 0 || input.DeleteTransferForHousingDue)
            {
                var housingPaymentPlanForTransfer = await _housingPaymentPlanRepository.GetAll()
                    .Where(p => p.HousingId == input.Id &&
                                p.HousingPaymentPlanType == HousingPaymentPlanType.Transfer &&
                                p.FirstHousingDueTransferIsResidentOrOwner != null).FirstOrDefaultAsync();

                if (housingPaymentPlanForTransfer != null) //Eski devir hareketinden gelen tutarlar geri alınıyor
                {
                    if (input.DeleteTransferForHousingDue)
                    {
                        await _housingPaymentPlanManager.DeleteAsync(housingPaymentPlanForTransfer);
                    }

                    housing = housingPaymentPlanForTransfer.CreditOrDebt == CreditOrDebt.Debt
                        ? Housing.DecreaseBalance(housing, housingPaymentPlanForTransfer.Amount,
                            housingPaymentPlanForTransfer.FirstHousingDueTransferIsResidentOrOwner.GetValueOrDefault())
                        : Housing.IncreaseBalance(housing, housingPaymentPlanForTransfer.Amount,
                            housingPaymentPlanForTransfer.FirstHousingDueTransferIsResidentOrOwner
                                .GetValueOrDefault());
                }

                //Yeni devir hareketi tutarları bakiyeye ekleniyor
                housing = input.TransferIsDebt
                    ? Housing.IncreaseBalance(housing, input.TransferAmount.GetValueOrDefault(),
                        input.TransferIsForResidentOrOwner)
                    : Housing.DecreaseBalance(housing, input.TransferAmount.GetValueOrDefault(),
                        input.TransferIsForResidentOrOwner);

                if (housingPaymentPlanForTransfer == null) //Yeni kayıt
                {
                    var housingPaymentPlan = input.TransferIsDebt
                        ? HousingPaymentPlan.CreateDebt(
                            SequentialGuidGenerator.Instance.Create()
                            , AbpSession.GetTenantId()
                            , null
                            , housing
                            , input.TransferIsForResidentOrOwner
                            , null
                            , input.TransferDateString.StringToDateTime()
                            , input.TransferAmount.GetValueOrDefault()
                            , input.TransferDescription
                            , HousingPaymentPlanType.Transfer
                            , null
                            , input.TransferIsForResidentOrOwner
                            , activePeriod.Id
                        )
                        : HousingPaymentPlan.CreateCredit(
                            SequentialGuidGenerator.Instance.Create()
                            , AbpSession.GetTenantId()
                            , housing
                            , input.TransferIsForResidentOrOwner
                            , null
                            , input.TransferDateString.StringToDateTime()
                            , input.TransferAmount.GetValueOrDefault()
                            , input.TransferDescription
                            , null
                            , HousingPaymentPlanType.Transfer
                            , null
                            , input.TransferIsForResidentOrOwner
                            , activePeriod.Id
                        );

                    await _housingPaymentPlanManager.CreateAsync(housingPaymentPlan);
                }
                else //Güncelleme ise
                {
                    HousingPaymentPlan.UpdateForFirstHousingDueTransfer(housingPaymentPlanForTransfer,
                        input.TransferIsForResidentOrOwner
                        , input.TransferAmount.GetValueOrDefault(),
                        input.TransferIsDebt,
                        input.TransferDateString.StringToDateTime(),
                        input.TransferDescription);
                }
            }

            return ObjectMapper.Map<HousingDto>(housing);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var housing = await _housingManager.GetAsync(input.Id);
            await _housingManager.DeleteAsync(housing);
        }

        public async Task<UpdateHousingDto> GetHousingForUpdate(Guid id)
        {
            CheckGetAllPermission();
            var housing = await _housingRepository.GetAsync(id);
            var updateHousingDto = new UpdateHousingDto //TODO object mapper'a çevir
            {
                Apartment = housing.Apartment,
                Id = housing.Id,
                BlockId = housing.BlockId,
                HousingCategoryId = housing.HousingCategoryId,
                TenantIsResiding = housing.TenantIsResiding,
                DeleteTransferForHousingDue = false
            };

            var housingPaymentPlanForTransfer = await _housingPaymentPlanRepository.GetAll()
                .Where(p => p.HousingId == id && p.HousingPaymentPlanType == HousingPaymentPlanType.Transfer &&
                            p.FirstHousingDueTransferIsResidentOrOwner != null).FirstOrDefaultAsync();

            if (housingPaymentPlanForTransfer != null)
            {
                // housingPaymentPlanForTransfer.SetAbsAmount();
                updateHousingDto.TransferAmount = housingPaymentPlanForTransfer.Amount;
                updateHousingDto.TransferDateString = housingPaymentPlanForTransfer.Date.ToString("yyyyMMdd");
                updateHousingDto.TransferDescription = housingPaymentPlanForTransfer.Description;
                updateHousingDto.TransferIsDebt =
                    housingPaymentPlanForTransfer.CreditOrDebt == CreditOrDebt.Debt;
                updateHousingDto.TransferIsForResidentOrOwner = housingPaymentPlanForTransfer
                    .FirstHousingDueTransferIsResidentOrOwner
                    .GetValueOrDefault();
            }

            return updateHousingDto;
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
                    select new { housing, housingPerson, person })
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

        public async Task<List<LookUpDto>> GetHousingLookUpAsync(HousingLookUpFilter filter)
        {
            CheckGetAllPermission();
            IQueryable<Housing> query;

            if (filter.PersonId.HasValue)
            {
                query = from h in _housingRepository.GetAll().Include(p => p.Block)
                    join hp in _housingPersonRepository.GetAll() on h.Id equals hp.HousingId
                    where hp.PersonId == filter.PersonId.Value
                    select h;
            }
            else
            {
                query = _housingRepository.GetAll().Include(p => p.Block).Select(p => p);
            }

            if (filter.PaymentCategoryId.HasValue)
            {
                var paymentCategory = await _paymentCategoryManager.GetAsync(filter.PaymentCategoryId.Value);

                var housingCategoryIds = await _paymentCategoryManager.GetHousingCategories(paymentCategory.Id);

                query = query.WhereIf(paymentCategory.IsHousingDue,
                    p => housingCategoryIds.Contains(p.HousingCategoryId));
            }

            return
                (from l in await query.ToListAsync()
                    select new LookUpDto(l.Id.ToString(), l.GetName())).ToList();
        }

        public async Task AddPersonAsync(CreateHousingPersonDto input)
        {
            CheckUpdatePermission();
            var housing = await _housingManager.GetAsync(input.HousingId);
            foreach (var personId in input.PeopleIds)
            {
                var person = await _personManager.GetAsync(personId);
                await _housingManager.AddPersonAsync(housing, person, input.IsTenant, input.Contact);
            }
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
    }
}