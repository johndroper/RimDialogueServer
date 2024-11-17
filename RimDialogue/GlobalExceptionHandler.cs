using Amazon.Runtime;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net.Mail;

namespace RimDialogue
{
  public class GlobalExceptionHandler(IHostEnvironment env, ILogger<GlobalExceptionHandler> logger)
  : Microsoft.AspNetCore.Diagnostics.IExceptionHandler
  {
    public static void HandleException(Exception ex, HttpContext httpContext)
    {
      var stackTrace = ex.StackTrace?.ToString();
      ErrorMail errorMail = new ErrorMail(
        ex.Message,
        stackTrace, 
        ex.Data,
        httpContext.Request.GetDisplayUrl(),
        httpContext.Request.Headers,
        httpContext.Connection.RemoteIpAddress?.ToString());
      var credentials = new BasicAWSCredentials("AKIAVMVXBHP6TA3S4475", "olZdcdY4VD152bSTSYJp6UP5aN7/lx4T5VCHgjJr");
      Amazon.SimpleEmailV2.AmazonSimpleEmailServiceV2Client client = new Amazon.SimpleEmailV2.AmazonSimpleEmailServiceV2Client(credentials, Amazon.RegionEndpoint.USEast1);
      var message = new Amazon.SimpleEmailV2.Model.Message
      {
        Body = new Amazon.SimpleEmailV2.Model.Body
        {
          Html = new Amazon.SimpleEmailV2.Model.Content
          {
            Charset = "UTF-8",
            Data = errorMail.TransformText()
          }
        },
        Subject = new Amazon.SimpleEmailV2.Model.Content
        {
          Charset = "UTF-8",
          Data = "Error Report"
        }
      };
      var sendEmailRequest = new Amazon.SimpleEmailV2.Model.SendEmailRequest
      {
        FromEmailAddress = "errors@proceduralproducts.com",
        Destination = new Amazon.SimpleEmailV2.Model.Destination
        {
          ToAddresses = new List<string> { "john.roper@gmail.com" },
        },
        Content = new Amazon.SimpleEmailV2.Model.EmailContent
        {
          Simple = message
        },
      };
      client.SendEmailAsync(sendEmailRequest);
    }

    public ValueTask<bool> TryHandleAsync(
          HttpContext httpContext,
          Exception exception,
          CancellationToken cancellationToken)
    {
      HandleException(exception, httpContext);

      return ValueTask.FromResult(false);
    }
  }
}
