namespace CrucibleAI.Tests.Constants;

public enum ChatClientFailure
{
    NetworkError,
    RequestTimeout,
    TooManyRequests,
    ServerError,
    ServiceUnavailable,
    Timeout,
    BrokenStream,
    ClientSideTimeout,
    SdkTooManyRequests,
    SdkBadGateway,
    AzureRequestTimeout,
    AzureServiceUnavailable,
    WrappedSdkTooManyRequests,
    BadRequest,
    Unauthorized,
    InvalidOperation,
    InvalidArgument,
    SdkBadRequest,
    AzureUnauthorized,
    WrappedSdkUnauthorized,
}
