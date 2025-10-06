using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RimDialogue.Core;
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

    public abstract Task<IActionResult> Login(string clientId);

    public abstract Task<IActionResult> ProcessDialogue<DataT, TemplateT>(
      string action,
      string initiatorJson,
      string dataJson)
    where DataT : RimDialogue.Core.InteractionData.DialogueData
    where TemplateT : IPromptTemplate<DataT>, new();

    public abstract Task<IActionResult> ProcessTargetDialogue<DataT, TemplateT>(
     string action,
     string initiatorJson,
     string dataJson,
     string? targetJson)
        where DataT : DialogueTargetData
        where TemplateT : ITargetPromptTemplate<DataT>, new();

    public abstract Task<IActionResult> ProcessTwoPawn<DataT, TemplateT>(
      string action,
      string initiatorJson,
      string recipientJson,
      string dataJson)
        where DataT : RimDialogue.Core.InteractionData.DialogueData
        where TemplateT : IRecipientPromptTemplate<DataT>, new();

    public abstract Task<IActionResult> ProcessTwoPawnTarget<DataT, TemplateT>(
     string action,
     string initiatorJson,
     string recipientJson,
     string dataJson,
     string? targetJson)
        where DataT : DialogueTargetData
        where TemplateT : IRecipientTargetPromptTemplate<DataT>, new();

    [HttpPost]
    public async Task<IActionResult> RecentIncidentChitchat(string initiatorJson, string recipientJson, string chitChatJson, string? targetJson)
    {
      return await ProcessTwoPawnTarget<DialogueDataIncident, ChitChatRecentIncidentTemplate>("RecentIncidentChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }

    [HttpPost]
    [Obsolete]
    public async Task<IActionResult> RecentBattleChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataBattle, ChitChatBattleTemplate>("RecentBattleChitchat", initiatorJson, recipientJson, chitChatJson);
    }

    [HttpPost]
    public async Task<IActionResult> Battle(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataBattle, ChitChatBattleTemplate>("Battle", initiatorJson, recipientJson, chitChatJson);
    }

    [HttpPost]
    public async Task<IActionResult> GameConditionChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataCondition, ChitChatGameConditionTemplate>("GameConditionChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> MessageChitchat(string initiatorJson, string recipientJson, string chitChatJson, string? targetJson)
    {
      return await ProcessTwoPawnTarget<DialogueDataMessage, ChitChatMessageTemplate>("MessageChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }
    [HttpPost]
    public async Task<IActionResult> AlertChitchat(string initiatorJson, string recipientJson, string chitChatJson, string? targetJson)
    {
      return await ProcessTwoPawnTarget<DialogueDataAlert, ChitChatAlertTemplate>("AlertChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }
    [HttpPost]
    public async Task<IActionResult> IdeologyChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<RimDialogue.Core.InteractionData.DialogueData, ChitChatIdeologyTemplate>("IdeologyChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> SkillChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataSkill, ChitChatSkillTemplate>("SkillsChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> ColonistChitchat(string initiatorJson, string recipientJson, string chitChatJson, string? targetJson)
    {
      return await ProcessTwoPawnTarget<DialogueTargetData, ChitChatColonistTemplate>("ColonistChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }
    [HttpPost]
    public async Task<IActionResult> HealthChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataHealth, ChitChatHealthTemplate>("HealthChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> ApparelChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataApparel, ChitChatApparelTemplate>("ApparelChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> NeedsChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataNeed, ChitChatNeedTemplate>("NeedsChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> FamilyChitchat(string initiatorJson, string recipientJson, string chitChatJson, string targetJson)
    {
      return await ProcessTwoPawnTarget<DialogueDataFamily, ChitChatFamilyTemplate>("FamilyChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }
    [HttpPost]
    public async Task<IActionResult> Dialogue(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<RimDialogue.Core.InteractionData.DialogueData, TwoPawnPromptTemplate>("DialogueChitchat", initiatorJson, recipientJson, chitChatJson);
    }

    [HttpPost]
    public async Task<IActionResult> DialogueSinglePawn(string initiatorJson, string chitChatJson)
    {
      return await ProcessDialogue<RimDialogue.Core.InteractionData.DialogueData, OnePawnPromptTemplate>("DialogueChitchat", initiatorJson, chitChatJson);
    }

    [HttpPost]
    public async Task<IActionResult> DialogueSinglePawnTarget(string initiatorJson, string chitChatJson, string targetJson)
    {
      return await ProcessTargetDialogue<DialogueTargetData, OnePawnTargetPromptTemplate>("DialogueChitchat", initiatorJson, chitChatJson, targetJson);
    }

    [HttpPost]
    public async Task<IActionResult> WeatherChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataWeather, ChitChatWeatherTemplate>("WeatherChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> WeaponChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataWeapon, ChitChatWeaponTemplate>("WeaponChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> FactionChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataFaction, ChitChatFactionTemplate>("FactionChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> AppearanceChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataAppearance, ChitChatAppearanceTemplate>("AppearanceChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> AnimalChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataAnimal, ChitChatAnimalTemplate>("AnimalChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> RoomChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataRoom, ChitChatRoomTemplate>("RoomChitchat", initiatorJson, recipientJson, chitChatJson);
    }
    [HttpPost]
    public async Task<IActionResult> DeadPawn(string initiatorJson, string recipientJson, string chitChatJson, string targetJson)
    {
      return await ProcessTwoPawnTarget<DialogueDataDeadPawn, DeadPawnTemplate>("DeadPawn", initiatorJson, recipientJson, chitChatJson, targetJson);
    }

    [HttpPost]
    public async Task<IActionResult> BattleLog(string initiatorJson, string chitChatJson)
    {
      return await ProcessDialogue<BattleLogData, BattleLogTemplate>("BattleLog", initiatorJson, chitChatJson);
    }

    [HttpPost]
    public async Task<IActionResult> Thought(string initiatorJson, string chitChatJson, string targetJson)
    {
      return await ProcessTargetDialogue<ThoughtData, ThoughtTemplate>("Thought", initiatorJson, chitChatJson, targetJson);
    }

    [HttpPost]
    public async Task<IActionResult> QuestChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessTwoPawn<DialogueDataQuest, ChitChatQuestTemplate>("Quest", initiatorJson, recipientJson, chitChatJson);
    }

    [HttpPost]
    public async Task<IActionResult> GetScenarioPrompt(string clientId, string scenarioText, string modelName = "Default")
    {
      string prompt = "Write a one sentence prompt that succinctly describes this scenario from a Rimworld game from the third person perspective:\r\n" + scenarioText;
      DialogueResponse response = await RunPrompt(clientId, prompt, modelName);
      return Json(response);
    }

    [HttpPost]
    public async Task<IActionResult> GetCharacterPrompt(string clientId, string pawnJson, string modelName = "Default")
    {
      var pawnData = JsonConvert.DeserializeObject<PawnData>(pawnJson);
      if (pawnData == null)
        throw new Exception("pawnData is null.");
      PawnTemplate pawnTemplate = new PawnTemplate(pawnData);
      string prompt = pawnTemplate.TransformText();
      DialogueResponse response = await RunPrompt(clientId, prompt, modelName);
      return Json(response);
    }

    public abstract Task<DialogueResponse> RunPrompt(
      string clientId,
      string prompt,
      string modelName,
      [CallerMemberName] string? callerName = null);

  }
}
