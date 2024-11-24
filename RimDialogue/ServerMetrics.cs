namespace RimDialogue
{
  public class ClientMetrics
  {
    private int requestCount = 0;
    private long totalInputLength = 0;
    private long totalOutputLength = 0;

    public ClientMetrics(string ip, int inputLength, int outputLength)
    {
      IP = ip;
      AddRequest(inputLength, outputLength);
    }
    public string IP { get; private set; }

    public int RequestCount
    {
      get { return requestCount; }
    }
    public void AddRequest(int inputLength, int outputLength)
    {
      Interlocked.Increment(ref requestCount);
      Interlocked.Add(ref totalInputLength, inputLength);
      Interlocked.Add(ref totalOutputLength, outputLength);
    }
    public long TotalInputLength
    {
      get { return totalInputLength; }
    }
    public long TotalOutputLength
    {
      get { return totalOutputLength; }
    }
  }

  public static class ServerMetrics
  {
    private static int requestCount = 0;
    private static DateTime startTime = DateTime.Now;
    private static Dictionary<string, ClientMetrics> requestLog = [];
    private static long totalInputLength = 0;
    private static long totalOutputLength = 0;
    private static int totalErrors = 0;

    public static int TotalErrors
    {
      get { return totalErrors; }
    }

    public static void IncrementErrors()
    {
      Interlocked.Increment(ref totalErrors);
    }

    public static int RequestCount
    {
      get { return requestCount; }
    }
    public static void AddRequest(string? ip, int inputLength, int outputLength)
    {
      Interlocked.Add(ref totalInputLength, inputLength);
      Interlocked.Add(ref totalOutputLength, outputLength);
      Interlocked.Increment(ref requestCount);
      if (ip == null)
        return;
      lock (requestLog)
      {
        if (requestLog.TryGetValue(ip, out ClientMetrics? value))
          value.AddRequest(inputLength, outputLength);
        else
          requestLog.Add(ip, new ClientMetrics(ip, inputLength, outputLength));
      }
    }
    public static DateTime StartTime
    {
      get { return startTime; }
    }
    public static TimeSpan Uptime
    {
      get { return DateTime.Now - startTime; }
    }
    public static float RequestsPerSecond
    {
      get { return requestCount / (float)Uptime.TotalSeconds; }
    }
    public static float RequestsPerMinute
    {
      get { return requestCount / (float)Uptime.TotalMinutes; }
    }
    public static float RequestsPerHour
    {
      get { return requestCount / (float)Uptime.TotalHours; }
    }
    public static long TotalInputLength
    {
      get { return totalInputLength; }
    }
    public static float AverageInputLength
    {
      get { return totalInputLength / (float)requestCount; }
    }
    public static float AverageInputPerMinute
    {
      get { return totalInputLength / (float)Uptime.TotalMinutes; }
    }
    public static long TotalOutputLength
    {
      get { return totalOutputLength; }
    }
    public static float AverageOutputLength
    {
      get { return totalOutputLength / (float)requestCount; }
    }
    public static float AverageOutputPerMinute
    {
      get { return totalOutputLength / (float)Uptime.TotalMinutes; }
    }
    public static int DistinctIPCount
    {
      get { return requestLog.Count; }
    }
    public static float AverageRequestsPerIP
    {
      get { return requestCount / (float)DistinctIPCount; }
    }
    public static List<KeyValuePair<string, int>> GetTopRequesters(int count)
    {
      return requestLog.OrderByDescending(keyValuePair => keyValuePair.Value.RequestCount)
        .Take(count)
        .Select(client => new KeyValuePair<string, int>(client.Key, client.Value.RequestCount))
        .ToList();
    }
    public static List<KeyValuePair<string, long>> GetTopRequestersByInput(int count)
    {
      return requestLog.OrderByDescending(keyValuePair => keyValuePair.Value.TotalInputLength)
        .Take(count)
        .Select(client => new KeyValuePair<string, long>(client.Key, client.Value.TotalInputLength))
        .ToList();
    }
    public static List<KeyValuePair<string, long>> GetTopRequestersByOutput(int count)
    {
      return requestLog.OrderByDescending(keyValuePair => keyValuePair.Value.TotalOutputLength)
        .Take(count)
        .Select(client => new KeyValuePair<string, long>(client.Key, client.Value.TotalOutputLength))
        .ToList();
    }
  }
}
