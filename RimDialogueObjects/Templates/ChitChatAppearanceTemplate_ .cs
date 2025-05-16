# nullable disable
using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public partial class ChitChatAppearanceTemplate : IRecipientPromptTemplate<DialogueDataAppearance>
  {
    public DialogueDataAppearance Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }
  }
}
