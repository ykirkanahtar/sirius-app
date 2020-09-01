using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;

namespace Sirius.AccountBooks
{
    public class AccountBookManager : IAccountBookManager
    {
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;

        public AccountBookManager(IRepository<AccountBook, Guid> accountBookRepository)
        {
            _accountBookRepository = accountBookRepository;
        }

        public async Task CreateAsync(AccountBook accountBook)
        {
            await _accountBookRepository.InsertAsync(accountBook);
        }
        
        public async Task UpdateAsync(AccountBook accountBook)
        {
            await _accountBookRepository.UpdateAsync(accountBook);
        }

        public async Task<AccountBook> GetAsync(Guid id)
        {
            var accountBook = await _accountBookRepository.GetAsync(id);
            if (accountBook == null)
            {
                throw new UserFriendlyException("Hesap hareketi bulunamadı");
            }
            return accountBook;
        }
    }
}
