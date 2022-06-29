using Microsoft.AspNetCore.Mvc;
using OSRC.AsyncLock.WebApi.Assets;
using StackExchange.Redis;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OSRC.AsyncLock.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisController : ControllerBase
    {
        public IConnectionMultiplexer Redis { get; }
        public DistributedAsyncCache DistributedAsyncCache { get; }

        public RedisController(
            IConnectionMultiplexer redis,
            DistributedAsyncCache distributedAsyncCache)
        {
            Redis = redis;
            DistributedAsyncCache = distributedAsyncCache;
        }

        // GET api/<RedisController>/5
        [HttpGet("{key}/{val}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public async Task<IActionResult> Get(string key, string val)
        {
            var db = Redis.GetDatabase();
            var foo = await db.SetContainsAsync(key, val);
            return Ok(foo.ToString());
        }

        // POST api/<RedisController>
        [HttpPost("{type}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public async Task<IActionResult> Post(string type, [FromBody] string[] keys)
        {
            var db = Redis.GetDatabase();
            var values = keys.Select(k => (RedisValue)k).ToArray();
            var foo = await db.SetAddAsync(type, values: values);
            return Ok(foo.ToString());
        }

        [HttpPost("{key}/{val}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public async Task<IActionResult> LongRunning(string key, string val, [FromBody] int delay)
        {
            using (await DistributedAsyncCache.AwaitAsync(key, val))
            {
                await Task.Delay(delay);
            }

            return Ok();
        }
    }
}
