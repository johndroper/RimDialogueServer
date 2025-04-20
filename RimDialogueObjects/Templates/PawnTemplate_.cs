using RimDialogue.Core;

namespace RimDialogueObjects.Templates
{
  public partial class PawnTemplate
  {
    public PawnTemplate(PawnData pawn)
    {
      Pawn = pawn ?? throw new ArgumentNullException(nameof(pawn), "Pawn cannot be null.");
    }

    public PawnData Pawn { get; set; }

  }
}
