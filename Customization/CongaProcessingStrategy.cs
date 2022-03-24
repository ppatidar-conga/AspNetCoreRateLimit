using AspNetCoreRateLimit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Customization
{
     public abstract class CongaProcessingStrategy : IProcessingStrategy
    {
        private readonly IRateLimitConfiguration _config;

        protected CongaProcessingStrategy(IRateLimitConfiguration config)
        {
            _config = config;
        }

        public abstract Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, ICounterKeyBuilder counterKeyBuilder, RateLimitOptions rateLimitOptions, CancellationToken cancellationToken = default);

        protected virtual string BuildCounterKey(ClientRequestIdentity requestIdentity, RateLimitRule rule, ICounterKeyBuilder counterKeyBuilder, RateLimitOptions rateLimitOptions)
        {
            var key = counterKeyBuilder.Build(requestIdentity, rule);

            if (rateLimitOptions.EnableEndpointRateLimiting && _config.EndpointCounterKeyBuilder != null)
            {
                key += _config.EndpointCounterKeyBuilder.Build(requestIdentity, rule);
            }

            return key;
        }
    }
}
