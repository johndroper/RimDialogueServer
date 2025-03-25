# nullable disable
using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public partial class ChitChatRecentIncidentTemplate : DialogueTargetTemplate<DialogueDataIncident>
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

    public PawnData Target { get; set; } = null;

    public string[] PastConversations { get; set; } = [];


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
