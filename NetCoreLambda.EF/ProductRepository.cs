using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCoreLambda.Abstractions;

namespace NetCoreLambda.EF
{
    public class ProductRepository : IProductRepository
    {
        public SampleDbContext Context { get; }

        public ProductRepository(SampleDbContext context)
        {
            Context = context;
        }

        public async Task<Product> GetProduct(int id)
        {
            return await Context.Products.SingleOrDefaultAsync(e => e.Id == id);
        }
    }
}
