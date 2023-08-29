using System;
namespace Core.Application.Pipelines.Caching
{
    public interface ICacheableRequest
	{
		string CacheKey { get; }
		bool ByPassCache { get; }
		TimeSpan? SlidingExpiration { get; }
	}
}

