using System;
using System.Collections.Generic;
using System.Text;

namespace Ecomm_project_1.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICompanyRepository company { get; }
        IProductRepository product { get; }
        ICategoryRepository category { get; }
        ICoverTypeRepository coverType { get; }
        IApplicationUserRepository applicationUser { get; }
        IShoppingCartRepository shoppingCart  { get; }
        IOrderDetailRepository orderDetail { get; }
        IOrderHeaderRepository orderHeader { get; }
        ISharedCartRepository sharedCart { get; }
        void save();
    }
}
