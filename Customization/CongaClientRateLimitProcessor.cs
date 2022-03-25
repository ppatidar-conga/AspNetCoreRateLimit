using AspNetCoreRateLimit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Customization
{
    public class CongaClientRateLimitProcessor : RateLimitProcessor, IRateLimitProcessor
    {
        private readonly ClientRateLimitOptions _options;
        private readonly IProcessingStrategy _processingStrategy;
        private readonly ILicencingManager licenceManager;
        private readonly IRateLimitStore<ClientRateLimitPolicy> _policyStore;
        private readonly ICounterKeyBuilder _counterKeyBuilder;

        public CongaClientRateLimitProcessor(
                ClientRateLimitOptions options,
                IClientPolicyStore policyStore,
                IProcessingStrategy processingStrategy,
                ILicencingManager licenceManager)
            : base(options)
        {
            _options = options;
            _policyStore = policyStore;
            _counterKeyBuilder = new ClientCounterKeyBuilder(options);
            _processingStrategy = processingStrategy;
            this.licenceManager = licenceManager;
        }

        public async Task<IEnumerable<RateLimitRule>> GetMatchingRulesAsync(ClientRequestIdentity identity, CancellationToken cancellationToken = default)
        {
            var tier =await  this.licenceManager.GetOrganizationRateLimitPlan(identity.ClientId);
            var policy = await _policyStore.GetAsync($"{_options.ClientPolicyPrefix}_{tier}", cancellationToken);

            return GetMatchingRules(identity, policy?.Rules);
        }

        public async Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, CancellationToken cancellationToken = default)
        {
            return await _processingStrategy.ProcessRequestAsync(requestIdentity, rule, _counterKeyBuilder, _options, cancellationToken);
        }
    }
}
