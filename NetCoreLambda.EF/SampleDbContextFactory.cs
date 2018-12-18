using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NetCoreLambda.EF
{
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
}