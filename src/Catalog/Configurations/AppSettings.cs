namespace Catalog.Configurations
{
    public sealed class BrokerOptions
    {
        public const string SectionName = "Broker";

        public required string Host { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public sealed class MediaOptions
    {
        public const string SectionName = "Media";

        public required string AccessKey { get; set; }
        public required string SecretKey { get; set; }
        public required string BucketName { get; set; }
        public required string Endpoint { get; set; }
    }

    public sealed class ShortenerOptions
    {
        public const string SectionName = "InternalServices:Shortener";

        public required string BaseUrl { get; set; }
    }
}
