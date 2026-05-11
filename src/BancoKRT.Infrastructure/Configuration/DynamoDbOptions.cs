namespace BancoKRT.Infrastructure.Configuration
{
    public sealed class DynamoDbOptions
    {
        public const string SectionName = "DynamoDb";

        public string TableName { get; set; } = "PixLimitAccounts";

        public string Region { get; set; } = "us-east-1";

        public string? ServiceUrl { get; set; }

        public string AccessKey { get; set; } = "test";

        public string SecretKey { get; set; } = "test";
    }
}
