using RimDialogueObjects;

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
