#nullable disable
using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public partial class ChitChatFactionTemplate : IRecipientPromptTemplate<DialogueDataFaction>
  {
    public static string DescribeGoodWill(int goodWill)
    {
      if (goodWill < -90) return "Enemies for life";
      if (goodWill < -70) return "Bitter enemies";
      if (goodWill < -50) return "Hostile";
      if (goodWill < -30) return "Very unfriendly";
      if (goodWill < -10) return "Unfriendly";
      if (goodWill < 0) return "Cold";
      if (goodWill < 10) return "Neutral";
      if (goodWill < 30) return "Friendly";
      if (goodWill < 50) return "Very friendly";
      if (goodWill < 70) return "Allies";
      if (goodWill < 90) return "Trusted allies";
      return "Best friends";
    }

    public DialogueDataFaction Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }

    public string GoodWill
    {
      get
      {
        return DescribeGoodWill(Data.GoodWill);
      }
    }

  }
}

