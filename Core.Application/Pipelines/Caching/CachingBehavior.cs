using System;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Core.Application.Pipelines.Caching
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>, ICacheableRequest
    {

        private readonly CacheSettings _cacheSettings;
        private readonly IDistributedCache _distributedCache;

        public CachingBehavior(CacheSettings cacheSettings, IDistributedCache distributedCache)
        {
            _cacheSettings = cacheSettings;
            _distributedCache = distributedCache;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request.ByPassCache)
            {
                return await next(); 
            }

            TResponse response;
            byte[]? cachedResponse = await _distributedCache.Get(request.CacheKey, cancellationToken);
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

            return response;
        }
    }
}

