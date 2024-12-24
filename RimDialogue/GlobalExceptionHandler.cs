using Amazon.Runtime;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using RimDialogueObjects;
using System.Net.Mail;

namespace RimDialogueLocal
{
  public class GlobalExceptionHandler()
  : Microsoft.AspNetCore.Diagnostics.IExceptionHandler
  {
    public ValueTask<bool> TryHandleAsync(
          HttpContext httpContext,
          Exception exception,
          CancellationToken cancellationToken)
    {
      Metrics.metricsData.serverMetrics.IncrementErrors();
      Console.WriteLine(exception.ToString());
      return ValueTask.FromResult(false);
    }
  }
}
