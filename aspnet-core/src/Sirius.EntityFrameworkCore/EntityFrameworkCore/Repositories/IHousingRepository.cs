using System;
using Abp.Domain.Repositories;
using Sirius.Housings;

namespace Sirius.EntityFrameworkCore.Repositories
{
    public interface IHousingRepository : IRepository<Housing, Guid>
    {
        
    }
}