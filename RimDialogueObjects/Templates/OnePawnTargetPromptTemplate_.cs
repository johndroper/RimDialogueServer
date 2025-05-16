# nullable disable
using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public partial class OnePawnTargetPromptTemplate : ITargetPromptTemplate<DialogueTargetData>
  {
    public DialogueTargetData Data { get; set; }
    public PawnData Initiator { get; set; }
    public Config Config { get; set; }
    public PawnData Target { get; set; } = null;
  }
}
