using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace Hasti.Framework.Endpoints.Logging.Extensions;

public class LoggingDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingDelegatingHandler> logger;

    public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
    {
        this.logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            logger.LogHttpResponse(response);

            return response;
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionRefused)
        {
            var hostWithPort = request.RequestUri.IsDefaultPort
                ? request.RequestUri.DnsSafeHost
                : $"{request.RequestUri.DnsSafeHost}:{request.RequestUri.Port}";

            logger.LogCritical(ex, "Unable to connect to {Host}. Please check the configuration to ensure the correct URL for the service has been configured.", hostWithPort);
        }

        return new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            RequestMessage = request
        };
    }
}
