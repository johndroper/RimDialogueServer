using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RimDialogue.Core;
using RimDialogueObjects;
using RimDialogueObjects.Templates;
using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace RimDialogueLocal.Controllers
{
  public class HomeController : DialogueController
  {

    public HomeController(
      IConfiguration Configuration,
      IMemoryCache memoryCache) : base(Configuration, memoryCache)
    {

    }



    public IActionResult Index()
    {
      return View();
    }

    protected bool IsOverRateLimit(Config config, string ipAddress, out float? rate)
    {
      var rateLimit = RequestRate.CheckRateLimit(ipAddress,
        config.RateLimit,
        config.RateLimitCacheMinutes,
        config.MinRateLimitRequestCount,
        MemoryCache,
        out rate);

      if (rateLimit)
        Log("Rate limited.", $"Rate {rate} > {config.RateLimit}");

      return rateLimit;
    }

    public override async Task<IActionResult> Login(string clientId)
    {
      await Task.Yield();

      var config = this.Configuration.Get<Config>() ?? throw new Exception("Config is null.");
      LoginData loginData = new()
      {
        tier = "Local",
        maxPromptLength = config.MaxPromptLength,
        maxResponseLength = config.MaxResponseLength,
        maxOutputWords = config.MaxOutputWords,
        rateLimit = config.RateLimit,
        rateLimitCacheMinutes = config.RateLimitCacheMinutes,
        minRateLimitRequestCount = config.MinRateLimitRequestCount,
        models = config.Models?.Select(m => m.Name).ToArray() ?? Array.Empty<string>()
      };

      return new JsonResult(loginData);
    }

    [NonAction]
    public override async Task<IActionResult> ProcessDialogue<DataT, TemplateT>(
      string action,
      string initiatorJson,
      string dataJson)
    {
      InitLog(action);
      try
      {
        if (dataJson == null)
          return new BadRequestResult();
        if (initiatorJson == null)
          throw new Exception("initiatorJson is null.");

        string? ipAddress = GetIp();
        Log(ipAddress, dataJson);
        var config = this.Configuration.Get<Config>();
        if (config == null)
          throw new Exception("config is null.");
        if (IsOverRateLimit(config, ipAddress, out float? rate))
          return new JsonResult(new DialogueResponse { RateLimited = true });
        try
        {
          //******Deserialization******
          DataT dialogueData = JsonConvert.DeserializeObject<DataT>(dataJson) 
            ?? throw new Exception("Can not deserialize dataJson");
          PawnData initiator = JsonConvert.DeserializeObject<PawnData>(initiatorJson) 
            ?? throw new Exception("Can not deserialize initiatorJson.");
           
          //******Prompt Generation******
          string truncatedPrompt = LlmHelper.Generate<DataT, TemplateT>(
            config,
            initiator,
            dialogueData,
            out bool inputTruncated,
            out string prompt);
          Log(truncatedPrompt, $"inputTruncated: {inputTruncated}");

          //******Response Generation******
          var result = await LlmHelper.GenerateResponse(truncatedPrompt, config, "Default");
          Log(result.Text);
          var dialogueResponse = LlmHelper.SerializeResponse(result.Text ?? string.Empty, Configuration, rate ?? 0f, out bool outputTruncated);
          if (outputTruncated)
            Log($"Response was truncated. Original length was {result.Text?.Length} characters.");
          Metrics.AddRequest(
            this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString(),
            truncatedPrompt.Length,
            result.Text?.Length ?? 0,
            inputTruncated,
            outputTruncated,
            null);
          Log("Metrics updated.");
          return new JsonResult(dialogueResponse);
        }
        catch (Exception ex)
        {
          Exception exception = new("An error occurred deserializing JSON.", ex);
          exception.Data.Add("initiatorJson", initiatorJson);
          exception.Data.Add("dataJson", dataJson);
          throw exception;
        }
      }
      catch (Exception ex)
      {
        LogException(ex);
        throw;
      }
    }
    public override async Task<IActionResult> ProcessTargetDialogue<DataT, TemplateT>(
      string action, 
      string initiatorJson,
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

        string? ipAddress = GetIp();
        Log(ipAddress, dataJson);
        var config = Configuration.Get<Config>();
        if (config == null)
          throw new Exception("config is null.");
        if (IsOverRateLimit(config, ipAddress, out float? rate))
          return new JsonResult(new DialogueResponse { RateLimited = true });
        DataT? dialogueData = default(DataT);
        PawnData? initiator = null;
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
          if (targetJson != null)
            target = JsonConvert.DeserializeObject<PawnData>(targetJson);
          Log("Deserialized dialogueData.");
        }
        catch (Exception ex)
        {
          Exception exception = new("An error occurred deserializing JSON.", ex);
          exception.Data.Add("initiatorJson", initiatorJson);
          exception.Data.Add("dataJson", dataJson);
          exception.Data.Add("targetJson", targetJson);
          throw exception;
        }

        //******Prompt Generation******
        string prompt = LlmHelper.Generate<DataT, TemplateT>(
          config,
          initiator,
          dialogueData,
          target,
          out bool inputTruncated);
        Log(prompt, $"inputTruncated: {inputTruncated}");
        //******Response Generation******
        var result = await LlmHelper.GenerateResponse(prompt, config, dialogueData.ModelName);
        Log(result.Text);
        var dialogueResponse = LlmHelper.SerializeResponse(result.Text ?? string.Empty, Configuration, rate ?? 0, out bool outputTruncated);
        if (outputTruncated)
          Log($"Response was truncated. Original length was {result.Text?.Length ?? 0} characters.");
        Metrics.AddRequest(
          this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString(),
          prompt.Length,
          result.Text?.Length ?? 0,
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
    public override async Task<IActionResult> ProcessTwoPawn<DataT, TemplateT>(
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
        var config = this.Configuration.Get<Config>();
        if (config == null)
          throw new Exception("config is null.");
        if (IsOverRateLimit(config, ipAddress, out float? rate))
          return new JsonResult(new DialogueResponse { RateLimited = true });
        
        try
        {
          //******Deserialization******
          DataT dialogueData = JsonConvert.DeserializeObject<DataT>(dataJson) ?? throw new Exception("Can not deserialize dataJson");
          if (dialogueData == null)
            throw new Exception("DialogueData is null.");
          PawnData initiator = JsonConvert.DeserializeObject<PawnData>(initiatorJson) ?? throw new Exception("Can not deserialize initiatorJson.");
          if (initiator == null)
            throw new Exception("Initiator is null.");
          PawnData recipient = JsonConvert.DeserializeObject<PawnData>(recipientJson) ?? throw new Exception("Can not deserialize recipientJson.");
          Log("Deserialized dialogueData.");

          //******Prompt Generation******
          string truncatedPrompt = LlmHelper.Generate<DataT, TemplateT>(
            config,
            initiator,
            recipient,
            dialogueData,
            out bool inputTruncated,
            out string prompt);
          Log(truncatedPrompt, $"inputTruncated: {inputTruncated}");

          //******Response Generation******
          var result = await LlmHelper.GenerateResponse(truncatedPrompt, config, dialogueData.ModelName);
          Log(result.Text);
          var dialogueResponse = LlmHelper.SerializeResponse(result.Text ?? String.Empty, Configuration, rate ?? 0f, out bool outputTruncated);
          if (outputTruncated)
            Log($"Response was truncated. Original length was {result.Text?.Length ?? 0} characters.");
          Metrics.AddRequest(
            this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString(),
            truncatedPrompt.Length,
            result.Text?.Length ?? 0,
            inputTruncated,
            outputTruncated,
            null);
          Log("Metrics updated.");
          return new JsonResult(dialogueResponse);
        }
        catch (Exception ex)
        {
          Exception exception = new("An error occurred deserializing JSON.", ex);
          exception.Data.Add("initiatorJson", initiatorJson);
          exception.Data.Add("recipientJson", recipientJson);
          exception.Data.Add("dataJson", dataJson);
          throw exception;
        }
      }
      catch (Exception ex)
      {
        LogException(ex);
        throw;
      }
    }

    [NonAction]
    public override async Task<IActionResult> ProcessTwoPawnTarget<DataT, TemplateT>(
      string action,
      string initiatorJson,
      string? recipientJson,
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
        var config = Configuration.Get<Config>();
        if (config == null)
          throw new Exception("config is null.");
        if (IsOverRateLimit(config, ipAddress, out float? rate))
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
          exception.Data.Add("targetJson", targetJson);
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
        var result = await LlmHelper.GenerateResponse(prompt, config, dialogueData.ModelName);
        Log(result.Text);
        var dialogueResponse = LlmHelper.SerializeResponse(result.Text ?? string.Empty, Configuration, rate ?? 0, out bool outputTruncated);
        if (outputTruncated)
          Log($"Response was truncated. Original length was {result.Text?.Length ?? 0} characters.");
        Metrics.AddRequest(
          this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString(),
          prompt.Length,
          result.Text?.Length ?? 0,
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

    public override async Task<DialogueResponse> RunPrompt(string clientId, string prompt, string modelName, [CallerMemberName] string? callerName = null)
    {
      InitLog(callerName);
      string? ipAddress = GetIp();
      Log(ipAddress, prompt);
      var config = Configuration.Get<Config>();
      if (config == null)
        throw new Exception("config is null.");
      if (IsOverRateLimit(config, ipAddress, out float? rate))
        throw new Exception($"Rate limit exceeded. Please try again later. Rate: {rate}");
      var results = await LlmHelper.GenerateResponse(prompt, config, modelName);
      Log(results.Text);
      return LlmHelper.SerializeResponse(results.Text ?? string.Empty, Configuration, rate ?? 0, out bool outputTruncated);
    }


  }
}
