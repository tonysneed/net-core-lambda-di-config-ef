using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using NetCoreLambda.DI;

namespace NetCoreLambda.EF.Design
{
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
}