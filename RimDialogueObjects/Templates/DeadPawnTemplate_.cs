# nullable disable
using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public partial class DeadPawnTemplate : IRecipientTargetPromptTemplate<DialogueDataDeadPawn>
  {
    public DialogueDataDeadPawn Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }
    public PawnData Target { get; set; } = null;

  }
}
