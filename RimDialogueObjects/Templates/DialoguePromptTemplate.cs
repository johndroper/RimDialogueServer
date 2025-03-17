using RimDialogue.Core;
using RimDialogueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RimDialogueObjects.Templates
{
  public static class DialoguePromptTemplateExtentions
  {
    public static string BoilerPlate<DataT>(this DialoguePromptTemplate<DataT> template) where DataT : RimDialogue.Core.InteractionData.DialogueData
    {
      return ChitChatBoilerPlate.Generate(
        template.Data,
        template.Initiator,
        template.Recipient,
        template.Config);
    }

    public static string TargetString<DataT>(this DialoguePromptTemplate<DataT> template) where DataT : RimDialogue.Core.InteractionData.DialogueTargetData
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
    public PawnData? Target { get; set; }
  }
}
