using RimDialogue.Core;

namespace RimDialogueObjects.Templates
{
  public partial class OnePawnTemplateSlim
  {
    public static string Generate(
      PawnData initiator,
      Config config)
    {
      OnePawnTemplateSlim template = new(
        initiator,
        config);
      return template.TransformText();
    }

    public PawnData Initiator { get; set; }

    public OnePawnTemplateSlim(
      PawnData initiator,
      Config tier)
    {
      Config = tier;
      Initiator = initiator;
    }

    public Config Config { get; set; }

  }
}
