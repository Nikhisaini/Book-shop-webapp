using Ecomm_project_1.DataAccess.Data;
using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecomm_project_1.DataAccess.Repository
{
    public class ShoppingCartRepository:Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _context;
        public ShoppingCartRepository(ApplicationDbContext context):base(context)
        {
            _context = context;
        }
    }
}
