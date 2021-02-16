using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using Sirius.Housings;

namespace Sirius.EntityFrameworkCore.Repositories
{
    public interface IHousingRepository : IRepository<Housing, Guid>
    {
    }
}