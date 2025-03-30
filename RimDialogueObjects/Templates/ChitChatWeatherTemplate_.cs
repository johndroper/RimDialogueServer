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

        public string GetThoughts(string name, string[] thoughts)
        {
            return string.Join(", ", thoughts);
        }

        public int MaxOutputWords
        {
            get
            {
                if (this.Data.MaxWords > Config.MaxOutputWords)
                    return Config.MaxOutputWords;
                else
                    return this.Data.MaxWords;
            }
        }
    }
}

