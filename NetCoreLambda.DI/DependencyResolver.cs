using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCoreLambda.Abstractions;
using NetCoreLambda.Configuration;
using NetCoreLambda.EF;

namespace NetCoreLambda.DI
{
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
}