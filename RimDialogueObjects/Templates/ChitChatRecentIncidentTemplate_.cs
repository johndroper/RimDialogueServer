﻿using RimDialogue.Core;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public partial class ChitChatRecentIncidentTemplate : DialoguePromptTemplate<DialogueDataIncident>
  {
    public InitiatorRecipientTemplate InitiatorRecipientTemplate
    {
      get
      {
        return new(Initiator, Recipient, Data.InitiatorOpinionOfRecipient, Data.RecipientOpinionOfInitiator, Config);
      }
    }
    public DialogueDataIncident Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }

    public PawnData? Target { get; set; } = null;

    public string TargetString
    {
      get
      {
        if (Target != null)
          return TargetTemplate.Generate(
            Initiator,
            Recipient,
            Target,
            Data.InitiatorOpinionOfTarget,
            Data.RecipientOpinionOfTarget,
            Config);
        else
          return string.Empty;
      }
    }

    public string GetThoughts(string name, string[] thoughts)
    {
      return string.Join(", ", thoughts);
    }
    public int MaxOutputWords
    {
      get
      {
        if (this.Data.MaxWords > Config.MaxOutputWords)
          return Config.MaxOutputWords;
        else
          return this.Data.MaxWords;
      }
    }
  }
}
