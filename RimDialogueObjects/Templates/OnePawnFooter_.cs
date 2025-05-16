using RimDialogue.Core;
using DialogueData = RimDialogue.Core.InteractionData.DialogueData;

namespace RimDialogueObjects.Templates
{

  public partial class OnePawnFooter
  {
    public static string Generate(DialogueData data, PawnData initiator, Config config)
    {
      OnePawnFooter template = new(data, initiator, config);
      return template.TransformText();
    }

    public OnePawnFooter(
      DialogueData data,
      PawnData initiator,
      Config config)
    {
      Data = data;
      Initiator = initiator;
      Config = config;
    }

    public DialogueData Data { get; set; }
    public PawnData Initiator { get; set; }
    public Config Config { get; set; }

  }
}
