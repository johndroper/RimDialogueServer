using RimDialogue.Core;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace RimDialogueObjects
{
  public partial class ChitChatRecentIncidentTemplate
  {
    public static Regex GetDataRegex = new(@"(\w+)\s\[([+-]?[0-9]+[.]?[0-9]+)\]", RegexOptions.Compiled);

    public static string Generate(
      Config config,
      PawnData initiator,
      PawnData recipient,
      ChitChatRecentIncidentData dialogueData, 
      out bool wasTruncated)
    {
      //******Prompt Generation******
      string? prompt = null;
      try
      {
        wasTruncated = false;
        if (dialogueData == null)
          throw new Exception("dialogueData is null.");
        ChitChatRecentIncidentTemplate promptTemplate = new(initiator, recipient, dialogueData, config);
        prompt = promptTemplate.TransformText();
        if (prompt == null)
          throw new Exception("Prompt is null.");
        int maxPromptLength = config.MaxPromptLength;
        if (prompt.Length > maxPromptLength)
        {
          wasTruncated = true;
          Console.WriteLine($"Prompt truncated to {maxPromptLength} characters. Original length was {prompt.Length} characters.");
          return prompt.Substring(0, maxPromptLength);
        }
        return prompt;
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred generating prompt.", ex);
        exception.Data.Add("dialogueData", JsonConvert.SerializeObject(dialogueData));
        throw exception;
      }
    }

    public ChitChatRecentIncidentTemplate(
      PawnData initiator,
      PawnData recipient,
      ChitChatRecentIncidentData data,
      Config tier)
    {
      Initiator = initiator;
      Recipient = recipient;
      Data = data;
      Config = tier;
    }

    public ChitChatRecentIncidentData Data { get; set; }

    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }

    public Config Config { get; set; }

    public string GetThoughts(string name, string[] thoughts)
    {
      return string.Join(", ", thoughts);
    }
    public int MaxOutputWords
    {
      get
      {
        if (this.Data.maxWords > Config.MaxOutputWords)
          return Config.MaxOutputWords;
        else
          return this.Data.maxWords;
      }
    }
  }
}
