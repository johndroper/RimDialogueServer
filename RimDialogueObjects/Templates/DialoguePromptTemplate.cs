using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public static class DialoguePromptTemplateExtensions
  {
    public static string BoilerPlate<DataT>(this DialoguePromptTemplate<DataT> template) where DataT : RimDialogue.Core.InteractionData.DialogueData
    {
      return ChitChatBoilerPlate.Generate(
        template.Data,
        template.Initiator,
        template.Recipient,
        template.Config);
    }

    public static string GetTarget<DataT>(this DialogueTargetTemplate<DataT> template) where DataT : DialogueTargetData
    {
      if (template.Target != null)
        return TargetTemplate.Generate(
          template.Initiator,
          template.Recipient,
          template.Target,
          template.Data.InitiatorOpinionOfTarget,
          template.Data.RecipientOpinionOfTarget,
          template.Config);
      else
        return string.Empty;
    }
  }

  public interface DialoguePromptTemplate<DataT> where DataT : RimDialogue.Core.InteractionData.DialogueData
  {
    public DataT Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }
    public string TransformText();
  }

  public interface DialogueTargetTemplate<DataT> : DialoguePromptTemplate<DataT> where DataT : DialogueTargetData
  {
    public PawnData? Target { get; set; }
  }
}
