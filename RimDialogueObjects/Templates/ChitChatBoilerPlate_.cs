using RimDialogue.Core;
using DialogueData = RimDialogue.Core.InteractionData.DialogueData;

namespace RimDialogueObjects.Templates
{

  public partial class ChitChatBoilerPlate
  {
    public static string Generate(DialogueData data, PawnData initiator, PawnData recipient, Config config)
    {
      ChitChatBoilerPlate template = new(data, initiator, recipient, config);
      return template.TransformText();
    }

    public ChitChatBoilerPlate(
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
