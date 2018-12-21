# AWS Lambda Function with Repository Pattern and EF Core

Demonstrates how to create a Lambda Function that uses the repository pattern with Entity Framework Core.

## Prerequisites

- .NET Core SDK
    - https://dotnet.microsoft.com/download/archives
    - Select version matching what is supported by AWS Lambda.

- Create a new Visual Studio solution and add a **global.json** file with the SDK version.
    - Open a command prompt at the solution root.
    - Enter and run: `dotnet new globaljson`
    - Add an existing item to the solution and select the global.json file.
    - Verify that the SDK version matches that which you previously installed.

## Create Database

1. Set connection string in appsettings.json.
    - This should point to an [AWS RDS instance](https://aws.amazon.com/rds/) which you have previously created.
    - Specify appropriate user id and password (or retrieve from [AWS Secrets Manager](https://aws.amazon.com/blogs/security/rotate-amazon-rds-database-credentials-automatically-with-aws-secrets-manager/)).

1. Set environment for creating database.
    - For Development:
    ```
    set ASPNETCORE_ENVIRONMENT=Development
    ```

    - For Prodution:
    ```
    set ASPNETCORE_ENVIRONMENT=Production
    ```

1. Change to directory where `SampleDbContextFactory` is located.

    ```
    cd NetCoreLambda.EF.Design
    ```

1. Create and seed database.

    ```
    dotnet ef database update
    ```

## Run the sample

- Press F5 to run **Mock Lambda Test Tool**.
    - Enter 1 for input.
    - Response should be JSON for a Product from the database.

## Setup Steps

1. Add a **NetCoreLambda.Abstractions** .NET Standard Class Library project to the solution
    - Add a `Product` class.
    - Add a `IProductRepository` interface.

    ```csharp
    public interface IProductRepository
    {
        Task<Product> GetProduct(int id);
    }
    ```

1. Add a **NetCoreLambda.EF** .NET Standard 2.0 Class Library.
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

1. Add a **NetCoreLambda.EF.Design** .NET Core Class Library version 2.1.
    - Add a `SampleDbContextFactory` class.

    ```csharp
    public class SampleDbContextFactory : IDesignTimeDbContextFactory<SampleDbContext>
    {
        public SampleDbContext CreateDbContext(string[] args)
        {
            // Get DbContext from DI system
            var resolver = new DependencyResolver
            {
                CurrentDirectory = Path.Combine(Directory.GetCurrentDirectory(), "../NetCoreLambda")
            };
            return resolver.ServiceProvider.GetService(typeof(SampleDbContext)) as SampleDbContext;
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

1. Add a connection string to **appsettings.Development.json** in the NetCore=Lambda project.

    ```json
    "ConnectionStrings": {
        "SampleDbContext": "Data Source=(localdb)\\MsSqlLocalDb;initial catalog=SampleDb;Integrated Security=True; MultipleActiveResultSets=True"
    }
    ```

1. Add a connection string to **appsettings.json** in the NetCore=Lambda project.

    ```json
    "SampleDbContext": "Data Source=sample-instance.xxx.eu-west-1.rds.amazonaws.com;initial catalog=SampleDb;User Id=xxx;Password=xxx; MultipleActiveResultSets=True"
    }
    ```

1. Add a **NetCoreLambda.DI** .NET Standard 2.0 Class Library.
    - Add a `DependencyResolver` class.

    ```csharp
    public class DependencyResolver
    {
        public IServiceProvider ServiceProvider { get; }
        public string CurrentDirectory { get; set; }
        public Action<IServiceCollection> RegisterServices { get; }

        public DependencyResolver(Action<IServiceCollection> registerServices = null)
        {
            // Set up Dependency Injection
            var serviceCollection = new ServiceCollection();
            RegisterServices = registerServices;
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register env and config services
            services.AddTransient<IEnvironmentService, EnvironmentService>();
            services.AddTransient<IConfigurationService, ConfigurationService>
                (provider => new ConfigurationService(provider.GetService<IEnvironmentService>())
                {
                    CurrentDirectory = CurrentDirectory
                });

            // Register DbContext class
            services.AddTransient(provider =>
            {
                var configService = provider.GetService<IConfigurationService>();
                var connectionString = configService.GetConfiguration().GetConnectionString(nameof(SampleDbContext));
                var optionsBuilder = new DbContextOptionsBuilder<SampleDbContext>();
                optionsBuilder.UseSqlServer(connectionString, builder => builder.MigrationsAssembly("NetCoreLambda.EF.Design"));
                return new SampleDbContext(optionsBuilder.Options);
            });

            // Register other services
            RegisterServices?.Invoke(services);
        }
    }
    ```

1. Add a `ProductRepository` property to the `Function` class with a constructor that sets it.

    ```csharp
    // Repository
    public IProductRepository ProductRepository { get; }

    public Function()
    {
        // Get Configuration Service from DI system
        var resolver = new DependencyResolver(ConfigureServices);
        ProductRepository = resolver.GetService<IProductRepository>();
    }

    // Use this ctor from unit tests that can mock IProductRepository
    public Function(IProductRepository productRepository)
    {
        ProductRepository = productRepository;
    }

    // Register services with DI system
    private void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IProductRepository, ProductRepository>();
    }
    ```

1. Flesh out the `FunctionHandler` class.

    ```csharp
    public async Task<Product> FunctionHandler(string input, ILambdaContext context)
    {
        int.TryParse(input, out var id);
        if (id == 0) return null;
        return await ProductRepository.GetProduct(id);
    }
    ```

