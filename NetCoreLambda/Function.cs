using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using NetCoreLambda.Abstractions;
using NetCoreLambda.DI;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace NetCoreLambda
{
    public class Function
    {
        // Repository
        public IProductRepository ProductRepository { get; }

        public Function()
        {
            // Get Configuration Service from DI system
            var resolver = new DependencyResolver();
            ProductRepository = resolver.ServiceProvider.GetService<IProductRepository>();
        }

        // Use this ctor from unit tests that can mock IProductRepository
        public Function(IProductRepository productRepository)
        {
            ProductRepository = productRepository;
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<Product> FunctionHandler(string input, ILambdaContext context)
        {
            int.TryParse(input, out var id);
            if (id == 0) return null;
            return await ProductRepository.GetProduct(id);
        }
    }
}
