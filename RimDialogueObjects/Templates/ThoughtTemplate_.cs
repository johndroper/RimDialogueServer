# nullable disable
using RimDialogue.Core;
using RimDialogue.Core.InteractionData;

namespace RimDialogueObjects.Templates
{
  public partial class ThoughtTemplate : ITargetPromptTemplate<ThoughtData>
  {
    public ThoughtData Data { get; set; }
    public PawnData Initiator { get; set; }
    public Config Config { get; set; }

    public PawnData Target { get; set; }

    private int? wordCount;
    public int WordCount
    {
      get
      {
        if (!wordCount.HasValue)
        {
          var maxWords = this.Data.MaxWords;
          if (maxWords > Config.MaxOutputWords)
            maxWords = Config.MaxOutputWords;
          var minWords = this.Data.MinWords;
          if (minWords > Config.MaxOutputWords)
            minWords = Config.MaxOutputWords;
          if (minWords >= maxWords)
            minWords = maxWords - 1;
          wordCount = Random.Shared.Next(minWords, maxWords);
        }
        return wordCount.Value;
      }
    }

    public string MoodOffset
    {
      get
      {
        if (Data.MoodOffset == 0)
          return "No impact";
        else
          return GetMoodDescription(Data.MoodOffset);
      }
    }

    public static string GetMoodDescription(float moodOffset)
    {
      if (moodOffset <= -27) return "Extremely demoralizing";
      else if (moodOffset <= -23) return "Deeply depressing";
      else if (moodOffset <= -19) return "Cripplingly unhappy";
      else if (moodOffset <= -15) return "Very negative";
      else if (moodOffset <= -11) return "Strongly unpleasant";
      else if (moodOffset <= -7) return "Clearly discouraging";
      else if (moodOffset <= -3) return "Noticeably upsetting";
      else if (moodOffset < 0) return "Mildly unpleasant";
      else if (moodOffset < 3) return "Small positive impact";
      else if (moodOffset < 7) return "Slightly uplifting";
      else if (moodOffset < 11) return "Moderately positive";
      else if (moodOffset < 15) return "Pleasantly encouraging";
      else if (moodOffset < 19) return "Clearly joyful";
      else if (moodOffset < 23) return "Very uplifting";
      else if (moodOffset < 27) return "Profoundly positive";
      else return "Overwhelmingly euphoric";
    }
  }
}
