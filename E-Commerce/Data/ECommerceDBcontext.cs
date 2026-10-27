
using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Data
{
    public class ECommerceDBcontext : DbContext
    {
        public ECommerceDBcontext(DbContextOptions options) : base(options)
        {
        }


        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
    }
}
