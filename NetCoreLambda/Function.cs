using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetCoreLambda.Abstractions;
using NetCoreLambda.EF;

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
            // Set up Dependency Injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Get Configuration Service from DI system
            ProductRepository = serviceProvider.GetService<IProductRepository>();
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

        private void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Register env and config services
            serviceCollection.AddTransient<IEnvironmentService, EnvironmentService>();
            serviceCollection.AddTransient<IConfigurationService, ConfigurationService>();

            // Register DbContext class
            serviceCollection.AddTransient(provider =>
            {
                var configService = provider.GetService<IConfigurationService>();
                var connectionString = configService.GetConfiguration()[$"ConnectionStrings:{nameof(SampleDbContext)}"];
                var optionsBuilder = new DbContextOptionsBuilder<SampleDbContext>();
                optionsBuilder.UseSqlServer(connectionString);
                return new SampleDbContext(optionsBuilder.Options);
            });

            // Register repository
            serviceCollection.AddTransient<IProductRepository, ProductRepository>();
        }
    }
}
