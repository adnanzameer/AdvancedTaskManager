namespace FoundationCore.Web.Business.Caching
{
    public interface ICacheManager<T>
    {
        T this[string key] { get; set; }
        T Get(string key, Func<T> factory);
        bool IsExists(string key);
        void Remove(string key);
        void InvalidateCachedObjects();
    }
}
