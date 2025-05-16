# nullable disable
using RimDialogue.Core;
using DialogueData = RimDialogue.Core.InteractionData.DialogueData;

namespace RimDialogueObjects.Templates
{
  public partial class ChitChatIdeologyTemplate : IRecipientTargetPromptTemplate<DialogueData>
  {
    public DialogueData Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }
    public PawnData Target { get; set; } = null;
  }
}
