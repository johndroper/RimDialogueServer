using RimDialogue.Core;
using DialogueData = RimDialogue.Core.InteractionData.DialogueData;

namespace RimDialogueObjects.Templates
{

  public partial class ChitChatFooter
  {
    public static string Generate(DialogueData data, PawnData initiator, PawnData recipient, Config config)
    {
      ChitChatFooter template = new(data, initiator, recipient, config);
      return template.TransformText();
    }

    public ChitChatFooter(
      DialogueData data,
      PawnData initiator,
      PawnData recipient,
      Config config)
    {
      Data = data;
      Initiator = initiator;
      Recipient = recipient;
      Config = config;
    }

    public DialogueData Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }

  }
}
