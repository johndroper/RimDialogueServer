﻿# nullable disable
using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public partial class ChitChatSkillTemplate : IRecipientTargetPromptTemplate<DialogueDataSkill>
  {
    public TwoPawnTemplateSlim TwoPawnTemplateSlim
    {
      get
      {
        return new(Initiator, Recipient, Data.InitiatorOpinionOfRecipient, Data.RecipientOpinionOfInitiator, Config);
      }
    }

    public DialogueDataSkill Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }
    public PawnData Target { get; set; } = null;

    public string[] PastConversations { get; set; } = [];

  }
}
