using System.Collections;

namespace RimDialogue
{
  partial class ErrorMail : ErrorMailBase
  {
    public ErrorMail(
        string message,
        string? stackTrace,
        Exception? innerException,
        IDictionary data,
        string url,
        IHeaderDictionary headers,
        string? ip)
    {
      Message = message;
      StackTrace = stackTrace;
      Data = data;
      Url = url;
      Headers = headers;
      Ip = ip;
      InnerException = innerException;
    }

    public Exception[] GetInnerExceptions()
    {
      var exceptions = new List<Exception>();
      Exception? innerException = InnerException;
      while(innerException != null)
      {
        exceptions.Add(innerException);
        innerException = innerException?.InnerException;
      }
      return exceptions.ToArray();
    }

    public Exception? InnerException { get; }
    public string Message { get; }
    public string? StackTrace { get; }
    public IDictionary Data { get; }
    public string Url { get; }
    public IHeaderDictionary Headers { get; }
    public string? Ip { get; }
  }
}
