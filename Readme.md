# AWS Lambda Function with EF and Repository Pattern

## Setup

1. Add a **NetCoreLambda.Abstractions** .NET Standard Class Library project to the solution
    - Add a `Product` class.
    - Add a `IProductRepository` interface.

    ```csharp
    public interface IProductRepository
    {
        Task<Product> GetProduct(int id);
    }
    ```

1. Add a **NetCoreLambda.EF** .NET Core Class Library version 2.1.
    - Add a `SampleDbContext` class

    ```csharp
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
    ```

    - Add a `SampleDbContextFactory` class.

    ```csharp
    public class SampleDbContextFactory : IDesignTimeDbContextFactory<SampleDbContext>
    {
        public SampleDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SampleDbContext>();
            optionsBuilder.UseSqlServer(
                @"Data Source=(localdb)\MsSqlLocalDb;initial catalog=SampleDb;Integrated Security=True; MultipleActiveResultSets=True");
            return new SampleDbContext(optionsBuilder.Options);
        }
    }
    ```

    - Add a `ProductRepository` class.

    ```csharp
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
    ```

1. Add a connection string to **appsettings.json** in the NetCore=Lambda project.

    ```json
    "ConnectionStrings": {
        "SampleDbContext": "Data Source=(localdb)\\MsSqlLocalDb;initial catalog=SampleDb;Integrated Security=True; MultipleActiveResultSets=True"
    }
    ```

1. Add a `ProductRepository` property to the `Function` class with a constructor that sets it.

    ```csharp
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
    ```

1. Add a ``ConfigureServices` method to the `Function` class.

    ```csharp
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
    ```

1. Flesh out the ``FunctionHandler` class.

    ```csharp
    public async Task<Product> FunctionHandler(string input, ILambdaContext context)
    {
        int.TryParse(input, out var id);
        if (id == 0) return null;
        return await ProductRepository.GetProduct(id);
    }
    ```

## Create Database

Open a command prompt and run the following two commands:

```
cd NetCoreLambda.EF
dotnet ef migrations add initial
dotnet ef database update
```
