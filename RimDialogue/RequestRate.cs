namespace RimDialogue
{
  public class RequestRate
  {
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
