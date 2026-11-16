using System.Net;
using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Fakes;

namespace CrucibleAI.Tests.Harnesses;

internal sealed class ChatClientFailures
{
    public static Exception Create(ChatClientFailure failure) => failure switch
    {
        ChatClientFailure.NetworkError => new HttpRequestException(ChatClientTexts.ClientFailure),
        ChatClientFailure.RequestTimeout => Status(HttpStatusCode.RequestTimeout),
        ChatClientFailure.TooManyRequests => Status(HttpStatusCode.TooManyRequests),
        ChatClientFailure.ServerError => Status(HttpStatusCode.InternalServerError),
        ChatClientFailure.ServiceUnavailable => Status(HttpStatusCode.ServiceUnavailable),
        ChatClientFailure.Timeout => new TimeoutException(ChatClientTexts.ClientFailure),
        ChatClientFailure.BrokenStream => new IOException(ChatClientTexts.ClientFailure),
        ChatClientFailure.ClientSideTimeout => new TaskCanceledException(ChatClientTexts.ClientFailure, new TimeoutException()),
        ChatClientFailure.SdkTooManyRequests => Sdk(HttpStatusCode.TooManyRequests),
        ChatClientFailure.SdkBadGateway => Sdk(HttpStatusCode.BadGateway),
        ChatClientFailure.AzureRequestTimeout => Azure(HttpStatusCode.RequestTimeout),
        ChatClientFailure.AzureServiceUnavailable => Azure(HttpStatusCode.ServiceUnavailable),
        ChatClientFailure.WrappedSdkTooManyRequests => new InvalidOperationException(ChatClientTexts.ClientFailure, Sdk(HttpStatusCode.TooManyRequests)),
        ChatClientFailure.SdkBadRequest => Sdk(HttpStatusCode.BadRequest),
        ChatClientFailure.AzureUnauthorized => Azure(HttpStatusCode.Unauthorized),
        ChatClientFailure.WrappedSdkUnauthorized => new InvalidOperationException(ChatClientTexts.ClientFailure, Sdk(HttpStatusCode.Unauthorized)),
        ChatClientFailure.BadRequest => Status(HttpStatusCode.BadRequest),
        ChatClientFailure.Unauthorized => Status(HttpStatusCode.Unauthorized),
        ChatClientFailure.InvalidOperation => new InvalidOperationException(ChatClientTexts.ClientFailure),
        ChatClientFailure.InvalidArgument => new ArgumentException(ChatClientTexts.ClientFailure),
        _ => throw new ArgumentOutOfRangeException(nameof(failure)),
    };

    private static ClientResultShapedException Sdk(HttpStatusCode status) => new(ChatClientTexts.ClientFailure, (int)status);

    private static RequestFailedShapedException Azure(HttpStatusCode status) => new((int)status, ChatClientTexts.ClientFailure);

    private static HttpRequestException Status(HttpStatusCode status) => new(ChatClientTexts.ClientFailure, null, status);
}
