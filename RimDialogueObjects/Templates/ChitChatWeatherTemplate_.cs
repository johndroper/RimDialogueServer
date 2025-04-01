#nullable disable
using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
    public partial class ChitChatWeatherTemplate : DialoguePromptTemplate<DialogueDataWeather>
    {
        public DialogueDataWeather Data { get; set; }
        public PawnData Initiator { get; set; }
        public PawnData Recipient { get; set; }
        public Config Config { get; set; }
    }
}

