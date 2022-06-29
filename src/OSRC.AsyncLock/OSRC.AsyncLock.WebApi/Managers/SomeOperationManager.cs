using StackExchange.Redis;

namespace OSRC.AsyncLock.WebApi.Managers
{
    public class SomeOperationManager
    {
        public IConnectionMultiplexer Redis { get; }

        public SomeOperationManager(IConnectionMultiplexer redis)
        {
            Redis = redis;
        }

        public async Task<string> GetAsync(string key)
        {
            await Task.Yield();
            return key;
        }

        public async Task<string> UpdateAsync(string key, string value)
        {
            await Task.Yield();

            return value;
        }

        public async Task<string> CreateAsync(string key, string value)
        {
            await Task.Yield();

            return value;
        }
    }
}
