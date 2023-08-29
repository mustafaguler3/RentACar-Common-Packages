using System;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Application.Pipelines.Caching
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>, ICacheableRequest
    {

        private readonly CacheSettings _cacheSettings;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

        public CachingBehavior(IDistributedCache distributedCache, IConfiguration configuration, ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cacheSettings = configuration.GetSection("CacheSettings").Get<CacheSettings>() ?? throw new InvalidOperationException();

            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request.ByPassCache)
            {
                return await next(); 
            }

            TResponse response;
            byte[]? cachedResponse = await _distributedCache.GetAsync(request.CacheKey, cancellationToken);
            if(cachedResponse != null)
            {
                response = JsonSerializer.Deserialize<TResponse>(Encoding.Default.GetString(cachedResponse));
            }else
            {
                response = await getResponseAndAddToCache(request,next,cancellationToken);
            }

            return response;
        }

        private async Task<TResponse?> getResponseAndAddToCache(TRequest request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            TResponse response = await next();

            TimeSpan slidingExpiration = request.SlidingExpiration ?? TimeSpan.FromDays(_cacheSettings.SlidingExpiration);
            DistributedCacheEntryOptions cacheOptions = new() { SlidingExpiration = slidingExpiration };

            byte[] serializeData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));

            await _distributedCache.SetAsync(request.CacheKey,serializeData,cacheOptions,cancellationToken);
            _logger.LogInformation($"Added to cache -> {request.CacheKey}");
            return response;
        }
    }
}

