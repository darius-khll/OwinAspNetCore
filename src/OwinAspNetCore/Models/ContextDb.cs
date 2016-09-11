using System.Data.Entity;

namespace OwinAspNetCore.Models
{
    public class ContextDb : DbContext
    {
        public ContextDb()
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
