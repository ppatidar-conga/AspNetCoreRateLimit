using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Customization
{
    public class CongaClientRateLimitMiddleware : RateLimitMiddleware<CongaClientRateLimitProcessor>
    {
        private readonly ILogger<ClientRateLimitMiddleware> _logger;
        private readonly ILicencingManager licenceManager;

        public CongaClientRateLimitMiddleware(RequestDelegate next,
            IProcessingStrategy processingStrategy,
            IOptions<ClientRateLimitOptions> options,
            IClientPolicyStore policyStore,
            IRateLimitConfiguration config,
            ILogger<ClientRateLimitMiddleware> logger, ILicencingManager licenceManager)
        : base(next, options?.Value, new CongaClientRateLimitProcessor(options?.Value, policyStore, processingStrategy, licenceManager), config)
        {
            _logger = logger;
            this.licenceManager = licenceManager;
        }

        protected override void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from ClientId {identity.ClientId} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.Count - rule.Limit}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}. MonitorMode: {rule.MonitorMode}");
        }
    }
}
