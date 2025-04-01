# nullable disable
using RimDialogue.Core;

namespace RimDialogueObjects.Templates
{
  public partial class ChitChatDialogueTemplate : DialoguePromptTemplate<RimDialogue.Core.InteractionData.DialogueData>
  {
    public RimDialogue.Core.InteractionData.DialogueData Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }
    public PawnData Target { get; set; } = null;
  }
}
