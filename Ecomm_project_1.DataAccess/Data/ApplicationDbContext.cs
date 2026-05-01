using Ecomm_project_1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecomm_project_1.DataAccess.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Catagory> Catagories { get; set; }
        public DbSet<CoverType> CoverTypes { get; set; }
        public DbSet<Product> products { get; set; }
        public DbSet<Company> companies { get; set; }
        public DbSet<ApplicationUser> applicationUsers { get; set; }
        public DbSet<ShoppingCart> shoppingCarts { get; set; }
        public DbSet<OrderHeader> orderHeaders { get; set; }
        public DbSet<OrderDetail> orderDetails { get; set; }
    }
}
