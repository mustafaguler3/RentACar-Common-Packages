using System;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Core.Application.Pipelines.Caching
{
	public class CacheRemovingBehavior<TRequest,TResponse>: IPipelineBehavior<TRequest,TResponse>
		where TRequest : IRequest<TResponse>,ICacheRemoverRequest
	{
        private readonly IDistributedCache _cache;

        public CacheRemovingBehavior(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request.ByPassCache)
            {
                return await next();
            }

            TResponse response = await next();

            if(request.CacheKey != null)
            {
                await _cache.RemoveAsync(request.CacheKey,cancellationToken);
            }

            return response;
        }
    }
}

