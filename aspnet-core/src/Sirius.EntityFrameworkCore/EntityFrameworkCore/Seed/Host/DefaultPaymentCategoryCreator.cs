using System.Linq;
using Abp;
using Microsoft.EntityFrameworkCore;
using Sirius.PaymentCategories;
using Sirius.Shared.Constants;
using Sirius.Shared.Enums;

namespace Sirius.EntityFrameworkCore.Seed.Host
{
    public class DefaultPaymentCategoryCreator
    {
        private readonly SiriusDbContext _context;

        public DefaultPaymentCategoryCreator(SiriusDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            AddPaymentCategoryIfNotExists(HousingDueType.RegularHousingDue, AppConstants.HousingDueString);
            AddPaymentCategoryIfNotExists(HousingDueType.TransferForRegularHousingDue, AppConstants.TransferForHousingDueString);
        }

        private void AddPaymentCategoryIfNotExists(HousingDueType housingDueType, string constant)
        {
            if (_context.PaymentCategories.IgnoreQueryFilters().Any(s => s.HousingDueType == housingDueType && s.TenantId == null))
            {
                return;
            }

            _context.PaymentCategories.Add(PaymentCategory.Create(SequentialGuidGenerator.Instance.Create(), null, constant, housingDueType, true));
            _context.SaveChanges();
        }
    }
}
