using System;
using NetCoreLambda.Abstractions;

namespace NetCoreLambda.Configuration
{
    public class EnvironmentService : IEnvironmentService
    {
        public EnvironmentService()
        {
            EnvironmentName = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AspnetCoreEnvironment)
                ?? Constants.Environments.Production;
        }

        public string EnvironmentName { get; set; }
    }
}