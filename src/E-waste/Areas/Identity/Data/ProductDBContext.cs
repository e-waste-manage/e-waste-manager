using Microsoft.EntityFrameworkCore;

namespace E_waste.Areas.Identity.Data
{
    public class ProductDBContext:DbContext
    {
        public ProductDBContext(DbContextOptions<ProductDBContext> options) : base(options)
        {
                
        }

        public DbSet<Product> Products { get; set;}
    }
}
