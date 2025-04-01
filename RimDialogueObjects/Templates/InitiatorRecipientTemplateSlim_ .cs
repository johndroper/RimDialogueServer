using RimDialogue.Core;

namespace RimDialogueObjects.Templates
{
  public partial class InitiatorRecipientTemplateSlim
  {
    public static string Generate(
      PawnData initiator, 
      PawnData recipient,
      int initiatorOpinionOfRecipient,
      int recipientOpinionOfInitiator,
      Config config)
    {
      InitiatorRecipientTemplateSlim template = new(
        initiator,
        recipient,
        initiatorOpinionOfRecipient,
        recipientOpinionOfInitiator,
        config);
              return template.TransformText();
    }

    public int InitiatorOpinionOfRecipient { get; set; }
    public int RecipientOpinionOfInitiator { get; set; }

    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }

    public InitiatorRecipientTemplateSlim(
      PawnData initiator,
      PawnData recipient,
      int initiatorOpinionOfRecipient,
      int recipientOpinionOfInitiator,
      Config tier)
    {
      Config = tier;
      Initiator = initiator;
      Recipient = recipient;
      InitiatorOpinionOfRecipient = initiatorOpinionOfRecipient;
      RecipientOpinionOfInitiator = recipientOpinionOfInitiator;
    }

    public Config Config { get; set; }

  }
}
