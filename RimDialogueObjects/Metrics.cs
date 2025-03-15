using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace RimDialogueObjects
{

  public class Metric
  {
    private int requestCount = 0;
    private long totalInputLength = 0;
    private long totalOutputLength = 0;
    private int totalErrors = 0;
    private int maxInputLength = 0;
    private int maxOutputLength = 0;
    private int wasRateLimitedCount = 0;
    private int inputTruncatedCount = 0;
    private int outputTruncatedCount = 0;
    private string name;

    public Metric(string name)
    {
      this.name = name;
    }
    public Metric(string name, int inputLength, int outputLength, bool inputTruncated, bool outputTruncated) : this(name)
    {
      AddRequest(inputLength, outputLength, inputTruncated, outputTruncated);
    }
    public void AddRequest(int inputLength, int outputLength, bool inputTruncated, bool outputTruncated)
    {
      this.IncrementRequests();
      this.AddInputLength(inputLength);
      this.AddOutputLength(outputLength);
      this.UpdateMaxInputLength(inputLength);
      this.UpdateMaxOutputLength(outputLength);
      if (inputTruncated)
        this.IncrementInputTruncated();
      if (outputTruncated)
        this.IncrementOutputTruncated();
    }
    public string Name
    {
      get { return name; }
      set { name = value; }
    }
    public int RequestCount
    {
      get { return requestCount; }
      set { requestCount = value; }
    }
    public int TotalErrors
    {
      get { return totalErrors; }
      set { totalErrors = value; }
    }
    public long TotalInputLength
    {
      get { return totalInputLength; }
      set { totalInputLength = value; }
    }
    public long TotalOutputLength
    {
      get { return totalOutputLength; }
      set { totalOutputLength = value; }
    }
    public int WasRateLimitedCount
    {
      get { return wasRateLimitedCount; }
      set { wasRateLimitedCount = value; }
    }
    public int InputTruncatedCount
    {
      get { return inputTruncatedCount; }
      set { inputTruncatedCount = value; }
    }
    public int OutputTruncatedCount
    {
      get { return outputTruncatedCount; }
      set { outputTruncatedCount = value; }
    }
    public int MaxInputLength
    {
      get { return maxInputLength; }
      set { maxInputLength = value; }
    }
    public int MaxOutputLength
    {
      get { return maxOutputLength; }
      set { maxOutputLength = value; }
    }
    public void IncrementRequests()
    {
      Interlocked.Increment(ref requestCount);
    }
    public void IncrementErrors()
    {
      Interlocked.Increment(ref totalErrors);
    }
    public void AddInputLength(int inputLength)
    {
      Interlocked.Add(ref totalInputLength, inputLength);
    }
    public void AddOutputLength(int outputLength)
    {
      Interlocked.Add(ref totalOutputLength, outputLength);
    }
    public void UpdateMaxInputLength(int inputLength)
    {
      if (inputLength > maxInputLength)
        Interlocked.Exchange(ref maxInputLength, inputLength);
    }
    public void UpdateMaxOutputLength(int outputLength)
    {
      if (outputLength > maxOutputLength)
        Interlocked.Exchange(ref maxOutputLength, outputLength);
    }
    public void IncrementWasRateLimited()
    {
      Interlocked.Increment(ref wasRateLimitedCount);
    }
    public void IncrementInputTruncated()
    {
      Interlocked.Increment(ref inputTruncatedCount);
    }
    public void IncrementOutputTruncated()
    {
      Interlocked.Increment(ref outputTruncatedCount);
    }
    public void WasRateLimited()
    {
      Interlocked.Increment(ref wasRateLimitedCount);
    }
    public float RequestsPerSecond
    {
      get { return requestCount / (float)Metrics.GetUpTime().TotalSeconds; }
    }
    public float RequestsPerMinute
    {
      get { return requestCount / (float)Metrics.GetUpTime().TotalMinutes; }
    }
    public float RequestsPerHour
    {
      get { return requestCount / (float)Metrics.GetUpTime().TotalHours; }
    }
    public float AverageInputLength
    {
      get { return totalInputLength / (float)requestCount; }
    }
    public float AverageInputPerMinute
    {
      get { return totalInputLength / (float)Metrics.GetUpTime().TotalMinutes; }
    }
    public float AverageOutputLength
    {
      get { return totalOutputLength / (float)requestCount; }
    }
    public float AverageOutputPerMinute
    {
      get { return totalOutputLength / (float)Metrics.GetUpTime().TotalMinutes; }
    }
    public float WasRateLimitedPerMinute
    {
      get { return wasRateLimitedCount / (float)Metrics.GetUpTime().TotalMinutes; }
    }
    public float InputTruncatedPerMinute
    {
      get { return inputTruncatedCount / (float)Metrics.GetUpTime().TotalMinutes; }
    }
    public float OutputTruncatedPerMinute
    {
      get { return outputTruncatedCount / (float)Metrics.GetUpTime().TotalMinutes; }
    }
  }

  public class MetricsData
  {
    public DateTime startTime = DateTime.Now;
    public Metric serverMetrics = new Metric("Server");
    public Dictionary<string, Metric> ipMetricsLog = [];
    public Dictionary<string, Metric> groupMetricsLog = [];
  }

  public static class Metrics
  {
    public static MetricsData metricsData;
    public static TimeSpan saveInterval = TimeSpan.FromMinutes(1);

    public static TimeSpan GetUpTime()
    {
      return DateTime.Now - metricsData.startTime;
    }

    static Metrics()
    {
      try
      {
        if (File.Exists("metricsData.json"))
        {
          string json = File.ReadAllText("metricsData.json");
          metricsData = JsonConvert.DeserializeObject<MetricsData>(json) ?? new MetricsData();
          return;
        }
        metricsData = new MetricsData();
        Timer timer = new Timer((object? state) =>
        {
          try
          {
            string json = JsonConvert.SerializeObject(metricsData);
            File.WriteAllText("metricsData.json", json);
          }
          catch (Exception ex)
          {
            Console.WriteLine($"Failed to save metrics data: {ex.ToString()}");
          }
        },
          null,
          0,
          (int)saveInterval.TotalMilliseconds);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Failed to load metrics data: {ex.ToString()}");
        metricsData = new MetricsData();
      }
    }
    public static void Reset()
    {
      metricsData = new MetricsData();
    }

    public static void WasRateLimited(string? ip, string? group)
    {
      metricsData.serverMetrics.IncrementWasRateLimited();
      if (ip != null)
      {
        lock (metricsData.ipMetricsLog)
        {
          if (metricsData.ipMetricsLog.TryGetValue(ip, out Metric? value))
            value.WasRateLimited();
        }
        if (group == null)
          return;
        lock (metricsData.groupMetricsLog)
        {
          if (metricsData.groupMetricsLog.TryGetValue(group, out Metric? value))
            value.WasRateLimited();
        }
      }
    }

    public static void AddRequest(string? ip, int inputLength, int outputLength, bool inputTruncated, bool outputTruncated, string? group)
    {
      metricsData.serverMetrics.AddRequest(inputLength, outputLength, inputTruncated, outputTruncated);

      if (ip != null)
      {
        lock (metricsData.ipMetricsLog)
        {
          if (metricsData.ipMetricsLog.TryGetValue(ip, out Metric? value))
            value.AddRequest(inputLength, outputLength, inputTruncated, outputTruncated);
          else
            metricsData.ipMetricsLog.Add(ip, new Metric(ip, inputLength, outputLength, inputTruncated, outputTruncated));
        }
      }

      if (group == null)
        return;
      lock (metricsData.groupMetricsLog)
      {
        if (metricsData.groupMetricsLog.TryGetValue(group, out Metric? value))
          value.AddRequest(inputLength, outputLength, inputTruncated, outputTruncated);
        else
          metricsData.groupMetricsLog.Add(group, new Metric(group, inputLength, outputLength, inputTruncated, outputTruncated));
      }

    }
    public static DateTime StartTime
    {
      get { return metricsData.startTime; }
    }
    public static int DistinctIPCount
    {
      get { return metricsData.ipMetricsLog.Count; }
    }
    public static float AverageRequestsPerIP
    {
      get { return metricsData.serverMetrics.RequestCount / (float)DistinctIPCount; }
    }
    public static List<KeyValuePair<string, int>> GetTopRequesters(int count)
    {
      return metricsData.ipMetricsLog.OrderByDescending(keyValuePair => keyValuePair.Value.RequestCount)
        .Take(count)
        .Select(client => new KeyValuePair<string, int>(client.Key, client.Value.RequestCount))
        .ToList();
    }
    public static List<KeyValuePair<string, long>> GetTopRequestersByInput(int count)
    {
      return metricsData.ipMetricsLog.OrderByDescending(keyValuePair => keyValuePair.Value.TotalInputLength)
        .Take(count)
        .Select(client => new KeyValuePair<string, long>(client.Key, client.Value.TotalInputLength))
        .ToList();
    }
    public static List<KeyValuePair<string, long>> GetTopRequestersByOutput(int count)
    {
      return metricsData.ipMetricsLog.OrderByDescending(keyValuePair => keyValuePair.Value.TotalOutputLength)
        .Take(count)
        .Select(client => new KeyValuePair<string, long>(client.Key, client.Value.TotalOutputLength))
        .ToList();
    }
  }
}
