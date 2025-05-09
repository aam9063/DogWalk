using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DogWalk_Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : RepositoryBase<T>, IRepository<T> where T : class
    {
        public GenericRepository(DogWalkDbContext context) : base(context)
        {
        }

    }
}
