using Ecomm_project_1.DataAccess.Data;
using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecomm_project_1.DataAccess.Repository
{
    public class SharedCartRepository:Repository<SharedCart>, ISharedCartRepository
    {
        private readonly ApplicationDbContext _context;
        public SharedCartRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
