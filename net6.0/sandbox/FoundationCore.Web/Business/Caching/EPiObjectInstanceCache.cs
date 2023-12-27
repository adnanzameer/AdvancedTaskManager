using EPiServer.Framework.Cache;
using EPiServer.ServiceLocation;

namespace FoundationCore.Web.Business.Caching
{
    [ServiceConfiguration(ServiceType = typeof(ICacheService), Lifecycle = ServiceInstanceScope.Transient)]
    public class EPiObjectInstanceCache : AbstractCacheService
    {
        private readonly ISynchronizedObjectInstanceCache _objectCacheService;

        public EPiObjectInstanceCache(ISynchronizedObjectInstanceCache objectCacheService)
        {
            _objectCacheService = objectCacheService;
        }

        public override TValue Get<TValue>(string cacheKey) => _objectCacheService.Get(cacheKey) as TValue;

        public override TValue Get<TValue>(string cacheKey, CacheEvictionPolicy cacheEvictionPolicy, Func<TValue> getItemCallback)
        {
            var item = Get<TValue>(cacheKey);
            if (item == null)
            {
                item = getItemCallback();
                _objectCacheService.Insert(cacheKey, item, cacheEvictionPolicy);
            }
            return item;
        }

        public override TValue Get<TValue, TId>(string cacheKeyFormat, TId id, int durationInMinutes, Func<TId, TValue> getItemCallback)
        {
            var cacheKey = string.Format(cacheKeyFormat, id);
            var item = Get<TValue>(cacheKey);
            if (item == null)
            {
                item = getItemCallback(id);
                _objectCacheService.Insert(cacheKey, item, new CacheEvictionPolicy(TimeSpan.FromMinutes(durationInMinutes), CacheTimeoutType.Absolute));
            }
            return item;
        }

        public override void Remove(string cacheKey) => _objectCacheService.Remove(cacheKey);
    }
}