using System;

namespace Sirius.Housings.Dto
{
    public class RemoveHousingPersonDto
    {
        public Guid HousingId { get; set; }
        public Guid PersonId { get; set; }
    }
}