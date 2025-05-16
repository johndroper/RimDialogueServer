using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public static class TemplateExtensions
  {
    public static string Header<DataT>(this IRecipientPromptTemplate<DataT> template) where DataT : RimDialogue.Core.InteractionData.DialogueData
    {
      return TwoPawnBoilerPlate.Generate(
        template.Data,
        template.Initiator,
        template.Recipient,
        template.Config);
    }

    public static string Header<DataT>(this IPromptTemplate<DataT> template) where DataT : RimDialogue.Core.InteractionData.DialogueData
    {
      return OnePawnHeader.Generate(
        template.Data,
        template.Initiator,
        template.Config);
    }

    public static string Footer<DataT>(this IRecipientPromptTemplate<DataT> template) where DataT : RimDialogue.Core.InteractionData.DialogueData
    {
      return TwoPawnFooter.Generate(
        template.Data,
        template.Initiator,
        template.Recipient,
        template.Config);
    }

    public static string Footer<DataT>(this IPromptTemplate<DataT> template) where DataT : RimDialogue.Core.InteractionData.DialogueData
    {
      return OnePawnFooter.Generate(
        template.Data,
        template.Initiator,
        template.Config);
    }

    public static string GetTarget<DataT>(this IRecipientTargetPromptTemplate<DataT> template) where DataT : DialogueTargetData
    {
      if (template.Target != null)
        return TwoPawnTargetTemplate.Generate(
          template.Initiator,
          template.Recipient,
          template.Target,
          template.Data.InitiatorOpinionOfTarget,
          template.Data.RecipientOpinionOfTarget,
          template.Config);
      else
        return string.Empty;
    }

    public static string GetTarget<DataT>(this ITargetPromptTemplate<DataT> template) where DataT : DialogueTargetData
    {
      if (template.Target != null)
        return OnePawnTargetTemplate.Generate(
          template.Initiator,
          template.Target,
          template.Data.InitiatorOpinionOfTarget,
          template.Config);
      else
        return string.Empty;
    }

    public static string GetPawn<DataT>(this IPromptTemplate<DataT> template) where DataT : RimDialogue.Core.InteractionData.DialogueData
    {
      return OnePawnTemplateSlim.Generate(
        template.Initiator,
        template.Config);
    }

    public static string GetPawns<DataT>(this IRecipientPromptTemplate<DataT> template) where DataT : RimDialogue.Core.InteractionData.DialogueData
    {
        return TwoPawnTemplateSlim.Generate(
          template.Initiator,
          template.Recipient,
          template.Data.InitiatorOpinionOfRecipient,
          template.Data.RecipientOpinionOfInitiator,
          template.Config);
    }
  }

  public interface IPromptTemplate<DataT> where DataT : RimDialogue.Core.InteractionData.DialogueData
  {
    public DataT Data { get; set; }
    public PawnData Initiator { get; set; }
    public Config Config { get; set; }
    public string TransformText();
  }

  public interface ITargetTemplate
  {
    public PawnData? Target { get; set; }
  }

  public interface ITargetPromptTemplate<DataT> : IPromptTemplate<DataT>, ITargetTemplate where DataT : RimDialogue.Core.InteractionData.DialogueTargetData;

  public interface IRecipientTemplate
  {
    public PawnData Recipient { get; set; }
  }

  public interface IRecipientPromptTemplate<DataT> : IPromptTemplate<DataT>, IRecipientTemplate where DataT : RimDialogue.Core.InteractionData.DialogueData;

  public interface IRecipientTargetPromptTemplate<DataT> : IRecipientPromptTemplate<DataT>, ITargetTemplate where DataT : RimDialogue.Core.InteractionData.DialogueData;
}
