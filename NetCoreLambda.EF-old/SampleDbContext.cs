using Microsoft.EntityFrameworkCore;
using NetCoreLambda.Abstractions;

namespace NetCoreLambda.EF
{
    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var product = new Product
            {
                Id = 1,
                ProductName = "Chai",
                UnitPrice = 10
            };
            modelBuilder.Entity<Product>().HasData(product);
        }
    }
}