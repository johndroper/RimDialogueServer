using RimDialogue.Core;

namespace RimDialogueObjects.Templates
{
  public partial class TwoPawnTargetTemplate
  {
    public static string Generate(
      PawnData initiator,
      PawnData recipient,
      PawnData target,
      int initiatorOpinionOfTarget,
      int recipientOpinionOfTarget,
      Config tier)
    {
      var targetTemplate = new TwoPawnTargetTemplate(
        initiator,
        recipient,
        target,
        initiatorOpinionOfTarget,
        recipientOpinionOfTarget,
        tier);
      return targetTemplate.TransformText();
    }

    public int InitiatorOpinionOfTarget { get; set; }
    public int RecipientOpinionOfTarget { get; set; }

    public PawnData Target { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }

    public TwoPawnTargetTemplate(
      PawnData initiator,
      PawnData recipient,
      PawnData target,
      int initiatorOpinionOfTarget,
      int recipientOpinionOfTarget,
      Config tier)
    {
      Config = tier;
      Initiator = initiator;
      Recipient = recipient;
      Target = target;
      InitiatorOpinionOfTarget = initiatorOpinionOfTarget;
      RecipientOpinionOfTarget = recipientOpinionOfTarget;
    }

    public Config Config { get; set; }

  }
}
