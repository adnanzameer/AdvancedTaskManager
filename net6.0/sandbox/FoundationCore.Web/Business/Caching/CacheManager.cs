using EPiServer.Framework.Cache;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace FoundationCore.Web.Business.Caching
{
    public class CacheManager<T> : ICacheManager<T>
        where T : class
    {

        private readonly TimeSpan _relativeExpiration;
        private readonly string[] _cacheDependencyKeys;
        private readonly IContentCacheKeyCreator _cacheVersionKey;
        private readonly ISynchronizedObjectInstanceCache _objectInstanceCache;

        public CacheManager() : this(new TimeSpan(0, 20, 0)) { }

        public CacheManager(TimeSpan relativeExpiration)
        {
            _objectInstanceCache = ServiceLocator.Current.GetInstance<ISynchronizedObjectInstanceCache>();
            _cacheVersionKey = ServiceLocator.Current.GetInstance<IContentCacheKeyCreator>();

            _cacheDependencyKeys = new[]
            {
                _cacheVersionKey.VersionKey
            };

            InitializeCacheDependency();
            _relativeExpiration = relativeExpiration;
        }

        public T this[string key]
        {
            get => _objectInstanceCache.Get(GetCacheKey(key)) as T;
            set
            {
                try
                {
                    var evictionPolicy = new CacheEvictionPolicy(_relativeExpiration, CacheTimeoutType.Absolute, _cacheDependencyKeys);
                    _objectInstanceCache.Insert(GetCacheKey(key), value, evictionPolicy);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger().Error($"CacheHelper<{typeof(T).FullName}>.Get({key}) failed to insert. VersionKey is {_cacheVersionKey}. InstanceKey is {InstanceKey}. Error: {ex}.");
                }
            }
        }

        public bool IsExists(string key)
        {
            var result = this[key];
            return result != null;
        }

        public T Get(string key, Func<T> factory)
        {
            var result = this[key];
            if (result == null)
            {
                result = factory();
                this[key] = result;
            }
            return result;
        }

        public void Remove(string key)
        {
            _objectInstanceCache.Remove(GetCacheKey(key));
        }

        public void InvalidateCachedObjects()
        {
            InitializeCacheDependency();
        }

        private string InstanceKey => typeof(T).FullName;

        private void InitializeCacheDependency()
        {
            _objectInstanceCache.Insert(InstanceKey, DateTime.Now.Ticks, null);
        }

        private static string GetCacheKey(string key)
        {
            return typeof(T).FullName + "_" + key;
        }
    }
}