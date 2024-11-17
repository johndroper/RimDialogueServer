using System.Collections;

namespace RimDialogue
{
  partial class ErrorMail : ErrorMailBase
  {
    public ErrorMail(
        string message,
        string? stacktrace,
        IDictionary data,
        string url,
        IHeaderDictionary headers,
        string? ip)
    {
      Message = message;
      Stacktrace = stacktrace;
      Data = data;
      Url = url;
      Headers = headers;
      Ip = ip;
    }

    public string Message { get; }
    public string? Stacktrace { get; }
    public IDictionary Data { get; }
    public string Url { get; }
    public IHeaderDictionary Headers { get; }
    public string? Ip { get; }
  }
}
