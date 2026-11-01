namespace Catalog.Infrastructure.Exceptions
{

    public sealed class ExternalServiceException(
        string serviceName,
        int statusCode,
        MessageCode code,
        Exception? innerException = null)
        : Exception($"External service '{serviceName}' failed.", innerException)
    {
        public string ServiceName { get; } = serviceName;
        public int StatusCode { get; } = statusCode;
        public MessageCode Code { get; } = code;
    }
}
