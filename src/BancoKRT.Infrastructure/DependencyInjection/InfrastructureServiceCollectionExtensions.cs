using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using BancoKRT.Domain.Interfaces;
using BancoKRT.Infrastructure.Configuration;
using BancoKRT.Infrastructure.Persistence.Initialization;
using BancoKRT.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BancoKRT.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<DynamoDbOptions>(configuration.GetSection(DynamoDbOptions.SectionName));

            services.AddSingleton<IAmazonDynamoDB>(sp =>
            {
                var options = sp
                    .GetRequiredService<
                        Microsoft.Extensions.Options.IOptions<DynamoDbOptions>>()
                    .Value;

                if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
                {
                    var localCredentials = new BasicAWSCredentials(
                        string.IsNullOrWhiteSpace(options.AccessKey) ? "test" : options.AccessKey,
                        string.IsNullOrWhiteSpace(options.SecretKey) ? "test" : options.SecretKey);

                    var localConfig = new AmazonDynamoDBConfig
                    {
                        ServiceURL = options.ServiceUrl,
                        UseHttp = true,
                        AuthenticationRegion = options.Region,
                        ProxyHost = null
                    };

                    return new AmazonDynamoDBClient(localCredentials, localConfig);
                }

                var credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);

                var config = new AmazonDynamoDBConfig
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region)
                };

                return new AmazonDynamoDBClient(credentials, config);
            });

            services.AddHostedService<DynamoDbTableInitializer>();
            services.AddScoped<IPixLimitAccountRepository, PixLimitAccountRepository>();

            return services;
        }
    }
}
