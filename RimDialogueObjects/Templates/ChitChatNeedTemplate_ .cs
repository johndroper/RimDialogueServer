# nullable disable
using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public partial class ChitChatNeedTemplate : IRecipientPromptTemplate<DialogueDataNeed>
  {
    public TwoPawnTemplateSlim TwoPawnTemplateSlim
    {
      get
      {
        return new(Initiator, Recipient, Data.InitiatorOpinionOfRecipient, Data.RecipientOpinionOfInitiator, Config);
      }
    }
    public DialogueDataNeed Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }

    public string NeedLevel
    {
      get
      {
        return TemplateHelper.DescribeNeedLevel(Data.NeedLabel, Data.NeedLevel);
      }
    }
  }
}
