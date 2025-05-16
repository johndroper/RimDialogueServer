using RimDialogue.Core;
using DialogueData = RimDialogue.Core.InteractionData.DialogueData;

namespace RimDialogueObjects.Templates
{

  public partial class OnePawnHeader
  {
    public static string Generate(DialogueData data, PawnData initiator, Config config)
    {
      OnePawnHeader template = new(data, initiator, config);
      return template.TransformText();
    }

    public OnePawnHeader(
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

    private int? wordCount;
    public int WordCount
    {
      get
      {
        if (!wordCount.HasValue)
        {
          var maxWords = this.Data.MaxWords;
          if (maxWords > Config.MaxOutputWords)
            maxWords = Config.MaxOutputWords;
          var minWords = this.Data.MinWords;
          if (minWords > Config.MaxOutputWords)
            minWords = Config.MaxOutputWords;
          if (minWords >= maxWords)
            minWords = maxWords - 1;
          wordCount = Random.Shared.Next(minWords, maxWords);
        }
        return wordCount.Value;
      }
    }
  }
}
