using AspNetCoreRateLimit;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Customization
{
    public class CongaRedisProcessingStrategy : CongaProcessingStrategy
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IRateLimitConfiguration _config;
        private readonly ILogger<CongaRedisProcessingStrategy> _logger;

        public CongaRedisProcessingStrategy(IConnectionMultiplexer connectionMultiplexer, IRateLimitConfiguration config, ILogger<CongaRedisProcessingStrategy> logger)
            : base(config)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentException("IConnectionMultiplexer was null. Ensure StackExchange.Redis was successfully registered");
            _config = config;
            _logger = logger;
        }

        static private readonly LuaScript _atomicIncrement = LuaScript.Prepare(
            "local count = redis.call(\"INCRBYFLOAT\", @key, tonumber(@delta))" +
            "local ttl = redis.call(\"TTL\", @key) " +
                "if ttl == -1 " +
                    "then redis.call(\"EXPIRE\", @key, @timeout) " +
                "end " +
            "return count");

        public override async Task<RateLimitCounter> ProcessRequestAsync(ClientRequestIdentity requestIdentity, RateLimitRule rule, ICounterKeyBuilder counterKeyBuilder, RateLimitOptions rateLimitOptions, CancellationToken cancellationToken = default)
        {
            var counterId = BuildCounterKey(requestIdentity, rule, counterKeyBuilder, rateLimitOptions);
            return await IncrementAsync(counterId, rule.PeriodTimespan.Value, _config.RateIncrementer);
        }

        public async Task<RateLimitCounter> IncrementAsync(string counterId, TimeSpan interval, Func<double> RateIncrementer = null)
        {
            var now = DateTime.UtcNow;
            var numberOfIntervals = now.Ticks / interval.Ticks;
            var intervalStart = new DateTime(numberOfIntervals * interval.Ticks, DateTimeKind.Utc);

           double count = 1;
            var _key = new RedisKey(counterId);
            _logger.LogDebug("Calling Lua script. {counterId}, {timeout}, {delta}", counterId, interval.TotalSeconds, 1D);
            //var count = await _connectionMultiplexer.GetDatabase().ScriptEvaluateAsync(_atomicIncrement, new { key = new RedisKey(counterId), timeout = interval.TotalSeconds, delta = RateIncrementer?.Invoke() ?? 1D });

            var db = _connectionMultiplexer.GetDatabase();
            //---Implementation 1
            count = await db.StringIncrementAsync(_key, RateIncrementer?.Invoke() ?? 1D);
            var ttl = await db.KeyTimeToLiveAsync(_key);
            if (ttl == null)
            {
                db.KeyExpire(_key, TimeSpan.FromSeconds(interval.TotalSeconds));
            }

            //---Implementation 2

            //count = await db.StringIncrementAsync(_key, RateIncrementer?.Invoke() ?? 1D);

            //var trans = db.CreateTransaction();
            //trans.AddCondition(Condition.k)
            //var ttl = await trans.KeyTimeToLiveAsync(_key);
            //if (ttl == null)
            //{
            //    await db.KeyExpireAsync(_key, TimeSpan.FromSeconds(interval.TotalSeconds));
            //}
            //trans.Execute();

            return new RateLimitCounter
            {
                Count = (double)count,
                Timestamp = intervalStart
            };
        }
    }
}
