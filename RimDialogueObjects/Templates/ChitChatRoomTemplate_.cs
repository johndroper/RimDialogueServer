#nullable disable
using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public partial class ChitChatRoomTemplate : DialoguePromptTemplate<DialogueDataRoom>
  {
    public DialogueDataRoom Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }

  }
}

