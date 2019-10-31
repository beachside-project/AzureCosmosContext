using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace AzureCosmosContext
{
    public class RequestChargeTrackRequestHandler : RequestHandler
    {
        private readonly ILogger<RequestChargeTrackRequestHandler> _logger;

        public RequestChargeTrackRequestHandler(ILogger<RequestChargeTrackRequestHandler> logger)
        {
            _logger = logger;
        }

        public override async Task<ResponseMessage> SendAsync(RequestMessage request, CancellationToken cancellation)
        {
            var response = await base.SendAsync(request, cancellation);
            _logger.LogInformation($"CosmosDB-RU ActivityId:{response.Headers?.ActivityId}; RequestCharge: {response.Headers?.RequestCharge.ToString(CultureInfo.InvariantCulture)};");
            return response;
        }
    }
}