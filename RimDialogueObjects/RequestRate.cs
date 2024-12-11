using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace RimDialogueObjects
{
  public class RequestRate
  {
    public static bool CheckRateLimit(string key, IConfiguration configuration, IMemoryCache memoryCache)
    {
      //******Rate Limiting******
      try
      {
        if (memoryCache.TryGetValue(key, out RequestRate? requestRate) && requestRate != null)
        {
          int minRateLimitRequestCount = configuration.GetValue<int>("MinRateLimitRequestCount", 5);
          if (requestRate.Count > minRateLimitRequestCount)
          {
            float rateLimit = configuration.GetValue<float>("RateLimit", 0.1f);
            var rate = requestRate.GetRate();
            if (rate > rateLimit)
            {
              Console.WriteLine($"{key} was rate limited to {rateLimit} requests per second. Current rate is {rate} requests per second.");
              return true;
            }
          }
          requestRate.Increment();
        }
        else
        {
          int rateLimitCacheMinutes = configuration.GetValue<int>("RateLimitCacheMinutes", 1);
          requestRate = new RequestRate(key);
          var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(rateLimitCacheMinutes));
          memoryCache.Set(key, requestRate, cacheEntryOptions);
        }
        return false;
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred in rate limiting.", ex);
        exception.Data.Add("key", key);
        throw exception;
      }
    }


    private int _count;

    public int Count
    {
      get
      {
        return _count;
      }
    }
    public string Key { get; set; }
    public DateTime FirstRequest { get; set; }

    public RequestRate(string key)
    {
      Key = key;
      _count = 1;
      FirstRequest = DateTime.UtcNow;
    }

    public float GetRate()
    {
      var totalSeconds = (DateTime.UtcNow - FirstRequest).TotalSeconds;
      if (totalSeconds == 0)
        return 0;
      return (float)Count / (float)totalSeconds;
    }

    public void Increment()
    {
      Interlocked.Increment(ref _count);
    }

  }
}
