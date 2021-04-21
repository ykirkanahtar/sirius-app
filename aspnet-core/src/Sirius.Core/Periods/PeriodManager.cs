using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.PaymentAccounts;
using Sirius.Shared.Enums;

namespace Sirius.Periods
{
    public class PeriodManager : IPeriodManager
    {
        private readonly IRepository<Period, Guid> _periodRepository;
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;
        private readonly IAccountBookManager _accountBookManager;
        private readonly IAccountBookPolicy _accountBookPolicy;
        private readonly IBalanceOrganizer _balanceOrganizer;

        public PeriodManager(IRepository<Period, Guid> periodRepository,
            IRepository<PaymentAccount, Guid> paymentAccountRepository,
            IAccountBookPolicy accountBookPolicy,
            IAccountBookManager accountBookManager, 
            IBalanceOrganizer balanceOrganizer)
        {
            _periodRepository = periodRepository;
            _paymentAccountRepository = paymentAccountRepository;
            _accountBookPolicy = accountBookPolicy;
            _accountBookManager = accountBookManager;
            _balanceOrganizer = balanceOrganizer;
        }

        public async Task<Period> GetAsync(Guid id)
        {
            var period = await _periodRepository.GetAsync(id);
            if (period == null)
            {
                throw new UserFriendlyException("Dönem bulunamadı");
            }

            return period;
        }

        public async Task CreateAsync(Period period, long sessionUserId)
        {
            //Yeni başlayacak dönemden daha yeni dönem varsa hata fırlatılıyor
            var newerPeriods = await _periodRepository.GetAll()
                .Where(p => p.SiteOrBlock == period.SiteOrBlock && p.StartDate >= period.StartDate).ToListAsync();

            if (newerPeriods.Any())
            {
                throw new UserFriendlyException("Sistemde daha yeni tarihli dönem bulunmaktadır.");
            }

            //Todo: yeni dönem tarihlerini kapsayan aidat ya da işletme defteri hareketi varsa hata fırlatılsın

            var activePeriod = await GetActivePeriod(false);

            if (activePeriod != null) //Önceki dönemden devir işlemleri yapılıyor
            {
                var paymentAccounts = await _paymentAccountRepository.GetAll().Where(p => p.Balance != 0).ToListAsync();
                foreach (var paymentAccount in paymentAccounts)
                {
                    var balance = paymentAccount.Balance;

                    var sameDayIndex = await _accountBookManager.GetSameDayIndexAsync(period.StartDate);

                    var accountBookTransferForOldPeriod =
                        await AccountBook.CreateTransferToNextPeriodForPaymentAccountAsync(
                            _accountBookPolicy
                            , SequentialGuidGenerator.Instance.Create()
                            , period.TenantId
                            , activePeriod.Id
                            , period.StartDate
                            , paymentAccount
                            , balance
                            , "Sonraki döneme devir"
                            , new List<AccountBookFile>()
                            , sessionUserId
                        );

                    var newIndex = sameDayIndex + 1;
                    accountBookTransferForOldPeriod.SetSameDayIndexManually(newIndex);
                    accountBookTransferForOldPeriod.SetToPaymentAccountCurrentBalance(0);

                    await _accountBookManager.CreateTransferForPaymentAccountAsync(accountBookTransferForOldPeriod,
                        paymentAccount, false);
                    
                    await _balanceOrganizer.GetOrganizedAccountBooksAsync(accountBookTransferForOldPeriod.ProcessDateTime,
                      new List<PaymentAccount>{ paymentAccount }  , new List<AccountBook> {accountBookTransferForOldPeriod}, null, null);
                    _balanceOrganizer.OrganizeAccountBookBalances();

                    var accountBookTransferForNewPeriod =
                        await AccountBook.CreateTransferFromPreviousPeriodForPaymentAccountAsync(
                            _accountBookPolicy
                            , SequentialGuidGenerator.Instance.Create()
                            , period.TenantId
                            , period.Id
                            , period.StartDate
                            , paymentAccount
                            , balance
                            , "Önceki dönemden devir"
                            , new List<AccountBookFile>()
                            , sessionUserId
                        );

                    newIndex = newIndex + 1;
                    accountBookTransferForNewPeriod.SetSameDayIndexManually(newIndex);
                    accountBookTransferForNewPeriod.SetToPaymentAccountCurrentBalance(balance);

                    await _accountBookManager.CreateTransferForPaymentAccountAsync(accountBookTransferForNewPeriod,
                        paymentAccount,
                        false);
                }
            }

            activePeriod?.ClosePeriod(period.StartDate);

            await _periodRepository.InsertAsync(period);
        }

        public async Task UpdateAsync(Period period)
        {
            await _periodRepository.UpdateAsync(period);
        }

        public async Task<Period> GetActivePeriod(bool nullCheck = true)
        {
            //Active olan dönem bulunuyor ve kayıtlarda varsa kapatılıyor
            var period = await _periodRepository.GetAll()
                .Where(p => p.IsActive)
                // .WhereIf(siteOrBlock == SiteOrBlock.Block,
                //     p => p.SiteOrBlock == SiteOrBlock.Block && p.BlockId == period.BlockId)
                // .WhereIf(siteOrBlock == SiteOrBlock.Site, p => p.SiteOrBlock == SiteOrBlock.Site)
                .SingleOrDefaultAsync();

            if (period == null && nullCheck)
            {
                throw new UserFriendlyException("Sistemde aktif bir dönem bulunamadı, lütfen dönem tanımlayın.");
            }

            return period;
        }
    }
}