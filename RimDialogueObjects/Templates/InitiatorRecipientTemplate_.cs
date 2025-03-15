using RimDialogue.Core;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace RimDialogueObjects.Templates
{
  public partial class InitiatorRecipientTemplate
  {
    //public static string Generate(Config config, DialogueData dialogueData, out bool wasTruncated)
    //{
    //  //******Prompt Generation******
    //  string? prompt = null;
    //  try
    //  {
    //    wasTruncated = false;
    //    if (dialogueData == null)
    //      throw new Exception("dialogueData is null.");
    //    PromptTemplate promptTemplate = new(dialogueData, config);
    //    prompt = promptTemplate.TransformText();
    //    if (prompt == null)
    //      throw new Exception("Prompt is null.");
    //    int maxPromptLength = config.MaxPromptLength;
    //    if (prompt.Length > maxPromptLength)
    //    {
    //      wasTruncated = true;
    //      Console.WriteLine($"Prompt truncated to {maxPromptLength} characters. Original length was {prompt.Length} characters.");
    //      return prompt.Substring(0, maxPromptLength);
    //    }
    //    return prompt;
    //  }
    //  catch (Exception ex)
    //  {
    //    Exception exception = new("An error occurred generating prompt.", ex);
    //    exception.Data.Add("dialogueData", JsonConvert.SerializeObject(dialogueData));
    //    throw exception;
    //  }
    //}

    public int InitiatorOpinionOfRecipient { get; set; }
    public int RecipientOpinionOfInitiator { get; set; }

    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }

    public InitiatorRecipientTemplate(
      PawnData initiator,
      PawnData recipient,
      int initiatorOpinionOfRecipient,
      int recipientOpinionOfInitiator,
      Config tier)
    {
      Config = tier;
      Initiator = initiator;
      Recipient = recipient;
      InitiatorOpinionOfRecipient = initiatorOpinionOfRecipient;
      RecipientOpinionOfInitiator = recipientOpinionOfInitiator;
    }

    public Config Config { get; set; }

  }
}
