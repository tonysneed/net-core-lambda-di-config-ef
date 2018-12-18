using Microsoft.Extensions.Configuration;

namespace NetCoreLambda.Abstractions
{
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
}