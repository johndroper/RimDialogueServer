using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using GroqSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OpenAI.Chat;
using RimDialogue.Core;
using RimDialogue.Core.InteractionData;
using RimDialogueObjects;
using RimDialogueObjects.Templates;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace RimDialogueLocal.Controllers
{
  public class HomeController : DialogueController
  {
    public HomeController(
      IConfiguration Configuration, 
      IMemoryCache memoryCache) : base(Configuration, memoryCache)
    {
 
    }

    private DateTime? _startTime;
    private DateTime? _lastLog = null;
    private StreamWriter? log;

    public IActionResult Index()
    {
      return View();
    }

    protected bool IsOverRateLimit(Config config, string ipAddress)
    {
      var rateLimit = RequestRate.CheckRateLimit(ipAddress,
        config.RateLimit,
        config.RateLimitCacheMinutes,
        config.MinRateLimitRequestCount,
        MemoryCache,
        out float? rate);

      if (rateLimit)
        Log("Rate limited.", $"Rate {rate} > {config.RateLimit}");

      return rateLimit;
    }

    [NonAction]
    public override async Task<IActionResult> ProcessDialogue<DataT, TemplateT>(
      string action,
      string initiatorJson,
      string recipientJson,
      string dataJson)
    {
      InitLog(action);
      try
      {
        if (dataJson == null)
          return new BadRequestResult();
        if (initiatorJson == null)
          throw new Exception("initiatorJson is null.");
        if (recipientJson == null)
          throw new Exception("recipientJson is null.");

        string? ipAddress = GetIp();
        Log(ipAddress, dataJson);
        var config = this.Configuration.GetSection("Options").Get<Config>();
        if (config == null)
          throw new Exception("config is null.");
        if (IsOverRateLimit(config, ipAddress))
          return new JsonResult(new DialogueResponse { RateLimited = true });
        DataT? dialogueData = default(DataT);
        PawnData? initiator = null;
        PawnData? recipient = null;
        //******Deserialization******
        try
        {
          dialogueData = JsonConvert.DeserializeObject<DataT>(dataJson);
          if (dialogueData == null)
            throw new Exception("DialogueData is null.");
          initiator = JsonConvert.DeserializeObject<PawnData>(initiatorJson);
          if (initiator == null)
            throw new Exception("Initiator is null.");
          recipient = JsonConvert.DeserializeObject<PawnData>(recipientJson);
          if (recipient == null)
            throw new Exception("Recipient is null.");
          Log("Deserialized dialogueData.");
        }
        catch (Exception ex)
        {
          Exception exception = new("An error occurred deserializing JSON.", ex);
          exception.Data.Add("initiatorJson", initiatorJson);
          exception.Data.Add("recipientJson", recipientJson);
          exception.Data.Add("dataJson", dataJson);
          throw exception;
        }
        //******Prompt Generation******
        string prompt = LlmHelper.Generate<DataT, TemplateT>(
          config,
          initiator,
          recipient,
          dialogueData,
          out bool inputTruncated);
        Log(prompt, $"inputTruncated: {inputTruncated}");
        //******Response Generation******
        string? text = await LlmHelper.GenerateResponse(prompt, Configuration);
        Log(text);
        var dialogueResponse = LlmHelper.SerializeResponse(text, Configuration, out bool outputTruncated);
        if (outputTruncated)
          Log($"Response was truncated. Original length was {text.Length} characters.");
        Metrics.AddRequest(
          this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString(),
          prompt.Length,
          text.Length,
          inputTruncated,
          outputTruncated,
          null);
        Log("Metrics updated.");
        return new JsonResult(dialogueResponse);
      }
      catch (Exception ex)
      {
        LogException(ex);
        throw;
      }
    }

    [NonAction]
    public override async Task<IActionResult> ProcessTargetDialogue<DataT, TemplateT>(
      string action, 
      string initiatorJson, 
      string recipientJson, 
      string dataJson, 
      string? targetJson) 
    {
      InitLog(action);
      try 
      {
        if (dataJson == null)
          return new BadRequestResult();
        if (initiatorJson == null)
          throw new Exception("initiatorJson is null.");
        if (recipientJson == null)
          throw new Exception("recipientJson is null.");

        string? ipAddress = GetIp();
        Log(ipAddress, dataJson);
        var config = Configuration.GetSection("Options").Get<Config>();
        if (config == null)
          throw new Exception("config is null.");
        if (IsOverRateLimit(config, ipAddress))
          return new JsonResult(new DialogueResponse { RateLimited = true });
        DataT? dialogueData = default(DataT);
        PawnData? initiator = null;
        PawnData? recipient = null;
        PawnData? target = null;
        //******Deserialization******
        try
        {
          dialogueData = JsonConvert.DeserializeObject<DataT>(dataJson);
          if (dialogueData == null)
            throw new Exception("DialogueData is null.");
          initiator = JsonConvert.DeserializeObject<PawnData>(initiatorJson);
          if (initiator == null)
            throw new Exception("Initiator is null.");
          recipient = JsonConvert.DeserializeObject<PawnData>(recipientJson);
          if (recipient == null)
            throw new Exception("Recipient is null.");
          if (targetJson != null)
            target = JsonConvert.DeserializeObject<PawnData>(targetJson);
          Log("Deserialized dialogueData.");
        }
        catch (Exception ex)
        {
          Exception exception = new("An error occurred deserializing JSON.", ex);
          exception.Data.Add("initiatorJson", initiatorJson);
          exception.Data.Add("recipientJson", recipientJson);
          exception.Data.Add("dataJson", dataJson);
          throw exception;
        }
        //******Prompt Generation******
        string prompt = LlmHelper.Generate<DataT, TemplateT>(
          config, 
          initiator, 
          recipient, 
          dialogueData, 
          target, 
          out bool inputTruncated);
        Log(prompt, $"inputTruncated: {inputTruncated}");
        //******Response Generation******
        string? text = await LlmHelper.GenerateResponse(prompt, Configuration);
        Log(text);
        var dialogueResponse = LlmHelper.SerializeResponse(text, Configuration, out bool outputTruncated);
        if (outputTruncated)
          Log($"Response was truncated. Original length was {text.Length} characters.");
        Metrics.AddRequest(
          this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString(),
          prompt.Length,
          text.Length,
          inputTruncated,
          outputTruncated,
          null);
        Log("Metrics updated.");
        return new JsonResult(dialogueResponse);
      }
      catch (Exception ex)
      {
        LogException(ex);
        throw;
      }
    }

    [HttpPost]
    public override async Task<IActionResult> GetDialogue(string dialogueDataJSON)
    {
      try
      {
        if (dialogueDataJSON == null)
          throw new Exception("dialogueDataJSON is null.");
        InitLog("GetDialogue");
        string? ipAddress = GetIp();
        Log(ipAddress, dialogueDataJSON);
        var config = Configuration.GetSection("Options").Get<Config>();
        if (config == null)
          throw new Exception("config is null.");
        if (IsOverRateLimit(config, ipAddress))
          return new JsonResult(new DialogueResponse { RateLimited = true });
        RimDialogue.Core.DialogueData? dialogueData = null;
        //******Deserialization******
        try
        {
          dialogueData = JsonConvert.DeserializeObject<RimDialogue.Core.DialogueData>(dialogueDataJSON);
          if (dialogueData == null)
            throw new Exception("DialogueData is null.");
          Log("Deserialized dialogueData.");
        }
        catch (Exception ex)
        {
          Exception exception = new("An error occurred deserializing JSON.", ex);
          exception.Data.Add("dialogueDataJSON", dialogueDataJSON);
          throw exception;
        }
        //******Prompt Generation******
        string prompt = PromptTemplate.Generate(config, dialogueData, out bool inputTruncated);
        Log(prompt, $"inputTruncated: {inputTruncated}");
        //******Response Generation******
        string? text = await LlmHelper.GenerateResponse(prompt, Configuration);
        Log(text);
        //****Remove everything between the start <think> tag and the end </think> tag ******
        if (Configuration.GetValue("RemoveThinking", false))
          text = Regex.Replace(text, "<think>(.|\n)*?</think>", "").Trim();
        //******Response Serialization******
        var dialogueResponse = LlmHelper.SerializeResponse(text, Configuration, out bool outputTruncated);
        if (outputTruncated)
          Log($"Response was truncated. Original length was {text.Length} characters.");
        //******Metrics******
        Metrics.AddRequest(
          this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString(),
          prompt.Length,
          text.Length,
          inputTruncated,
          outputTruncated,
          null);
        Log("Metrics updated.");
        return new JsonResult(dialogueResponse);
      }
      catch (Exception ex)
      {
        LogException(ex);
        throw;
      }
    }
  }
}
