# nullable disable
using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public partial class ChitChatHealthTemplate : DialoguePromptTemplate<DialogueDataHealth>
  {
    public InitiatorRecipientTemplate InitiatorRecipientTemplate
    {
      get
      {
        return new(Initiator, Recipient, Data.InitiatorOpinionOfRecipient, Data.RecipientOpinionOfInitiator, Config);
      }
    }

    public DialogueDataHealth Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }

    public string[] PastConversations { get; set; } = [];
  }
}
