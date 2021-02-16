using System;
using Abp.Application.Services.Dto;

namespace Sirius.Periods.Dto
{
    public class UpdatePeriodDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}