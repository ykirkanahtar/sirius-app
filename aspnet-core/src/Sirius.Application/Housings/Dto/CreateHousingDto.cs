using Abp.AutoMapper;

namespace Sirius.Housings.Dto
{
    [AutoMapTo(typeof(Housing))]

    public class CreateHousingDto
    {
        public string Block { get; set; }
        public string Apartment { get; set; }
    }
}
