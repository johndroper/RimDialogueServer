using RimDialogue.Core;

namespace RimDialogueObjects.Templates
{
  public partial class OnePawnTargetTemplate
  {
    public static string Generate(
      PawnData initiator,
      PawnData target,
      int initiatorOpinionOfTarget,
      Config tier)
    {
      var targetTemplate = new OnePawnTargetTemplate(
        initiator,
        target,
        initiatorOpinionOfTarget,
        tier);
      return targetTemplate.TransformText();
    }

    public int InitiatorOpinionOfTarget { get; set; }

    public PawnData Target { get; set; }
    public PawnData Initiator { get; set; }

    public OnePawnTargetTemplate(
      PawnData initiator,
      PawnData target,
      int initiatorOpinionOfTarget,
      Config tier)
    {
      Config = tier;
      Initiator = initiator;
      Target = target;
      InitiatorOpinionOfTarget = initiatorOpinionOfTarget;
    }

    public Config Config { get; set; }

  }
}
