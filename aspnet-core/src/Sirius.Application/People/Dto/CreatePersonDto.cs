using Abp.AutoMapper;

namespace Sirius.People.Dto
{
    [AutoMapTo(typeof(Person))]
    public class CreatePersonDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
    }
}
