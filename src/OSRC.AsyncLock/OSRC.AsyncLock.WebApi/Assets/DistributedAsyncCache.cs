using StackExchange.Redis;
using System.Collections.Concurrent;

namespace OSRC.AsyncLock.WebApi.Assets
{
    public class DistributedAsyncCache
    {
        protected ConcurrentDictionary<(string, string), AsyncLock> AsyncLockStore { get; } = new ConcurrentDictionary<(string, string), AsyncLock>();
        protected IConnectionMultiplexer Redis { get; }

        private AsyncLock GetLocker { get; } = new AsyncLock();
        private AsyncLock SetLocker { get; } = new AsyncLock();

        public DistributedAsyncCache(IConnectionMultiplexer redis)
        {
            Redis = redis;
        }

        public async Task<IDisposable> AwaitAsync(string type, string key, CancellationToken cancellationToken = default)
        {
            var db = Redis.GetDatabase();
            var isThere = await db.SetContainsAsync(type, key);
            if (isThere)
            {
                AsyncLock asyncLock;
                using (await GetLocker.AwaitAsync(cancellationToken))
                {
                    asyncLock = GetExistingAsyncLock((type, key));
                }
                return await asyncLock.AwaitAsync(cancellationToken);
            }

            var createRes = await db.SetAddAsync(type, key);
            if (createRes)
            {
                AsyncLock asyncLock;
                using (await SetLocker.AwaitAsync(cancellationToken))
                {
                    asyncLock = CreateAsyncLock((type, key));
                }
                return await asyncLock.AwaitAsync(cancellationToken);
            }

            else throw new InvalidOperationException();
        }

        protected AsyncLock GetExistingAsyncLock((string, string) typeKey)
        {
            AsyncLockStore.TryGetValue(typeKey, out var asyncLock);
            if (asyncLock is not null)
            {
                return asyncLock;
            }
            return CreateAsyncLock(typeKey);
        }

        protected AsyncLock CreateAsyncLock((string, string) typeKey)
        {
            var asyncLock = new AsyncLock();
            AsyncLockStore.TryAdd(typeKey, asyncLock);
            return asyncLock;
        }
    }
}
