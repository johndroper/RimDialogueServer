﻿# nullable disable
using RimDialogue.Core;

namespace RimDialogueObjects.Templates
{
  public partial class ChitChatDialogueTemplate : DialoguePromptTemplate<RimDialogue.Core.InteractionData.DialogueData>
  {

    public InitiatorRecipientTemplate InitiatorRecipientTemplate
    {
      get
      {
        return new(Initiator, Recipient, Data.InitiatorOpinionOfRecipient, Data.RecipientOpinionOfInitiator, Config);
      }
    }

    public RimDialogue.Core.InteractionData.DialogueData Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }
    public PawnData Target { get; set; } = null;

    public string[] PastConversations { get; set; } = [];
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
