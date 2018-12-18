using System.Threading.Tasks;

namespace NetCoreLambda.Abstractions
{
    public interface IProductRepository
    {
        Task<Product> GetProduct(int id);
    }
}
