using Microsoft.Extensions.Caching.Memory;

namespace RimDialogue
{
  public class Conversation
  {

    

    public static Dictionary<string, List<Conversation>> conversations = [];



    public Conversation(string pawn1, string pawn2, string interaction, string dialogue)
    {
      Pawn1 = pawn1;
      Pawn2 = pawn2;
      Interaction = interaction;
      Dialogue = dialogue;
    }

    public string Pawn1 { get; set; }
    public string Pawn2 { get; set; }
    public string Interaction { get; set; }
    public string Dialogue { get; set; }


  }
}
