using RimDialogue.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimDialogueObjects.Templates
{
  public interface DialoguePromptTemplate<DataT>
  {
    public DataT Data { get; set; }
    public PawnData Initiator { get; set; }
    public PawnData Recipient { get; set; }
    public Config Config { get; set; }
    public string TransformText();
    public PawnData? Target { get; set; }
  }
}
