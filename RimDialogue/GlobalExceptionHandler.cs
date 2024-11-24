using Amazon.Runtime;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net.Mail;

namespace RimDialogue
{
  public class GlobalExceptionHandler(IConfiguration Configuration) //IHostEnvironment env, ILogger<GlobalExceptionHandler> logger)
  : Microsoft.AspNetCore.Diagnostics.IExceptionHandler
  {
    public static void EmailException(Exception ex, HttpContext httpContext, string fromEmailAddress, List<string> toAddresses, string awsKey, string awsSecret)
    {
      var stackTrace = ex.StackTrace;
      ErrorMail errorMail = new ErrorMail(
        ex.Message,
        stackTrace,
        ex.InnerException,
        ex.Data,
        httpContext.Request.GetDisplayUrl(),
        httpContext.Request.Headers,
        httpContext.Connection.RemoteIpAddress?.ToString());
      var credentials = new BasicAWSCredentials(awsKey, awsSecret);
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
        FromEmailAddress = fromEmailAddress,
        Destination = new Amazon.SimpleEmailV2.Model.Destination
        {
          ToAddresses = toAddresses
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

      if (!bool.TryParse(Configuration["EmailErrors"], out bool emailErrors))
        emailErrors = false;
      if(emailErrors)
      {
        var errorEmailTo = Configuration["ErrorMailTo"];
        List<string>? toAddresses = null;
        if (!String.IsNullOrWhiteSpace(errorEmailTo))
          toAddresses = errorEmailTo.Split(',').ToList();
        var errorMailFrom = Configuration["ErrorMailFrom"];
        var awsKey = Configuration["AwsKey"];
        var awsSecret = Configuration["AwsSecret"];
        if (!String.IsNullOrWhiteSpace(errorMailFrom) && 
          !String.IsNullOrWhiteSpace(awsKey) && 
          !String.IsNullOrWhiteSpace(awsSecret) &&
          toAddresses != null &&
          toAddresses.Any())
          EmailException(exception, httpContext, errorMailFrom, toAddresses, awsKey, awsSecret);
      }
      return ValueTask.FromResult(false);
    }
  }
}
