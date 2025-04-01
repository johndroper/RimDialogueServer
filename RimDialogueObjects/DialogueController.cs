using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RimDialogue.Core.InteractionData;
using RimDialogueObjects.Templates;
using System.Runtime.CompilerServices;
using System.Text;

namespace RimDialogueObjects
{
  public abstract class DialogueController : Controller
  {
    private DateTime? _startTime;
    private DateTime? _lastLog = null;
    private StreamWriter? log;

    public IConfiguration Configuration { get; }
    public IMemoryCache MemoryCache { get; }

    public DialogueController(IConfiguration configuration, IMemoryCache memoryCache)
    {
      Configuration = configuration;
      MemoryCache = memoryCache;
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
      base.OnActionExecuted(context);
      if (log != null)
      {
        if (_startTime != null)
          log.WriteLine($"End of log. Total time taken: {DateTime.Now - _startTime}");
        log.Flush();
        log.Close();
      }
    }
    public void InitLog(string? action = null, [CallerMemberName] string? callerName = null)
    {
      if (action == null)
        action = callerName;
      if (log == null && Configuration["LoggingEnabled"]?.ToLower() == "true")
      {
        _startTime = DateTime.Now;
        if (!Directory.Exists("Output"))
          Directory.CreateDirectory("Output");
        log = System.IO.File.CreateText($"Output\\{action}-log-{DateTime.Now.Ticks}.txt");
        log.AutoFlush = true;
      }
    }
    public void Log(params string?[] data)
    {
      if (log == null)
        return;
      try
      {
        if (!Directory.Exists("Output"))
          Directory.CreateDirectory("Output");
        TimeSpan timeSinceLastLog = TimeSpan.Zero;
        if (_lastLog != null)
        {
          timeSinceLastLog = (TimeSpan)(DateTime.Now - _lastLog);
          log.WriteLine($"Time since last log: {timeSinceLastLog}");
        }
        foreach (var item in data)
        {
          if (item != null)
            log.WriteLine(item);
        }
        _lastLog = DateTime.Now;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error logging data.", ex);
        throw exception;
      }
    }

    protected string GetIp()
    {
      string? ipAddress = this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString();
      if (ipAddress == null)
        throw new Exception("IP Address is null.");
      return ipAddress;
    }

    public void LogException(Exception ex)
    {
      StringBuilder sb = new();
      foreach (var innerEx in ExceptionHelper.GetExceptionStack(ex))
      {
        sb.AppendLine(innerEx.Message);
        sb.AppendLine(innerEx.StackTrace);
        sb.AppendLine("Data:");
        foreach (var key in innerEx.Data.Keys)
        {
          sb.AppendLine($"{key}: {innerEx.Data[key]}");
        }
      }
      Log(sb.ToString());
    }

    public abstract Task<IActionResult> ProcessDialogue<DataT, TemplateT>(
      string action,
      string initiatorJson,
      string recipientJson,
      string dataJson)
        where DataT : RimDialogue.Core.InteractionData.DialogueData
        where TemplateT : DialoguePromptTemplate<DataT>, new();

    public abstract Task<IActionResult> ProcessTargetDialogue<DataT, TemplateT>(
     string action,
     string initiatorJson,
     string recipientJson,
     string dataJson,
     string? targetJson)
        where DataT : DialogueTargetData
        where TemplateT : DialogueTargetTemplate<DataT>, new();
    
    [HttpPost]
    public async Task<IActionResult> RecentIncidentChitchat(string initiatorJson, string recipientJson, string chitChatJson, string? targetJson)
    {
      return await ProcessTargetDialogue<DialogueDataIncident, ChitChatRecentIncidentTemplate>("RecentIncidentChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }
    [HttpPost]
    public async Task<IActionResult> RecentBattleChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<DialogueDataBattle, ChitChatBattleTemplate>("RecentBattleChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> GameConditionChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<DialogueDataCondition, ChitChatGameConditionTemplate>("GameConditionChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> MessageChitchat(string initiatorJson, string recipientJson, string chitChatJson, string? targetJson)
    {
      return await ProcessTargetDialogue<DialogueDataMessage, ChitChatMessageTemplate>("MessageChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }
    [HttpPost]
    public async Task<IActionResult> AlertChitchat(string initiatorJson, string recipientJson, string chitChatJson, string? targetJson)
    {
      return await ProcessTargetDialogue<DialogueDataAlert, ChitChatAlertTemplate>("AlertChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }
    [HttpPost]
    public async Task<IActionResult> IdeologyChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<RimDialogue.Core.InteractionData.DialogueData, ChitChatIdeologyTemplate>("IdeologyChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> SkillChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<DialogueDataSkill, ChitChatSkillTemplate>("SkillsChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> ColonistChitchat(string initiatorJson, string recipientJson, string chitChatJson, string? targetJson)
    {
      return await ProcessTargetDialogue<DialogueTargetData, ChitChatColonistTemplate>("ColonistChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }
    [HttpPost]
    public async Task<IActionResult> HealthChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<DialogueDataHealth, ChitChatHealthTemplate>("HealthChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> ApparelChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<DialogueDataApparel, ChitChatApparelTemplate>("ApparelChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> NeedsChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<DialogueDataNeed, ChitChatNeedTemplate>("NeedsChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> FamilyChitchat(string initiatorJson, string recipientJson, string chitChatJson, string targetJson)
    {
      return await ProcessTargetDialogue<DialogueDataFamily, ChitChatFamilyTemplate>("FamilyChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }
    [HttpPost]
    public async Task<IActionResult> Dialogue(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<RimDialogue.Core.InteractionData.DialogueData, ChitChatDialogueTemplate>("DialogueChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> WeatherChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<DialogueDataWeather, ChitChatWeatherTemplate>("WeatherChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    public async Task<IActionResult> FactionChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<DialogueDataFaction, ChitChatFactionTemplate>("FactionChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    public abstract Task<IActionResult> GetDialogue(string dialogueDataJSON);
  }
}
