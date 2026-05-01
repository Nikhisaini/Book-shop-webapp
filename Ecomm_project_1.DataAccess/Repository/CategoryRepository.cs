using Ecomm_project_1.DataAccess.Data;
using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Ecomm_project_1.DataAccess.Repository
{
    public class CategoryRepository : Repository<Catagory>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
      