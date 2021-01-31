using Sirius.Shared.Dtos;

namespace Sirius.PaymentCategories.Dto
{
    public class PaymentCategoryLookUpDto : LookUpDto
    {
        public PaymentCategoryLookUpDto(string value, string label, bool editInAccountBook) : base(value, label)
        {
            EditInAccountBook = editInAccountBook;
        }

        public bool EditInAccountBook { get; }
    }
}