using Ecomm_project_1.DataAccess.Data;
using Ecomm_project_1.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecomm_project_1.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            category = new CategoryRepository(context);
            coverType =new CoverTypeRepository(context);
            product = new ProductRepository(context);
            company = new CompanyRepository(context);
            applicationUser = new ApplicationUserRepository(context);
            shoppingCart = new ShoppingCartRepository(context);
            orderDetail = new OrderDetailRepository(context);
            orderHeader = new OrderHeaderRepository(context);
            sharedCart = new SharedCartRepository(context);
        }
        public IApplicationUserRepository applicationUser { private set; get; }
        public IShoppingCartRepository shoppingCart { private set; get; }
        public IOrderHeaderRepository orderHeader { private set; get; }
        public IOrderDetailRepository orderDetail { private set; get; }
        public ICompanyRepository company { private set; get; }
        public IProductRepository product { private set; get; }
        public ICategoryRepository category { private set; get; }

        public ICoverTypeRepository coverType { private set; get; }
        public ISharedCartRepository sharedCart { private set; get; }

        public void save()
        {
            _context.SaveChanges();
        }
    }
}
