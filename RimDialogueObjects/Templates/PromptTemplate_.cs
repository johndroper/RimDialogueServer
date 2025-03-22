using Newtonsoft.Json;
using RimDialogue.Core;
using System.Text.RegularExpressions;

namespace RimDialogueObjects.Templates
{
  public class FloatData
  {
    public FloatData(string label, float value)
    {
      Label = label;
      Value = value;
    }
    public string Label { get; set; }
    public float Value { get; set; }
  }


  public partial class PromptTemplate
  {
    public static Regex GetDataRegex = new(@"(\w+)\s\[([+-]?[0-9]+[.]?[0-9]+)\]", RegexOptions.Compiled);

    //private static Random rand = new Random();

    //private static string Rand(string[] input)
    //{
    //  return input[rand.Next(input.Length)];
    //}

    //private static Func<DialogueData, string?>[] factories =
    //{
    //  (data) => string.IsNullOrWhiteSpace(data.initiatorFullName) ? null : $"{data.initiatorNickName}'s full name is {data.initiatorFullName}",
    //  (data) => $"{data.recipientNickName}'s full name is {data.recipientFullName}",
    //  (data) => $"{data.initiatorNickName}'s gender is {data.initiatorGender}",
    //  (data) => $"{data.recipientNickName}'s gender is {data.recipientGender}",
    //  (data) => $"{data.initiatorNickName}'s race is {data.initiatorRace}",
    //  (data) => $"{data.recipientNickName}'s race is {data.recipientRace}",
    //  (data) => $"One of the precepts of {data.initiatorNickName}'s ideology is {Rand(data.initiatorIdeologyPrecepts)}",

    //};

    public static string Generate(Config config, DialogueData dialogueData, out bool wasTruncated)
    {
      //******Prompt Generation******
      string? prompt = null;
      try
      {
        wasTruncated = false;
        if (dialogueData == null)
          throw new Exception("dialogueData is null.");
        PromptTemplate promptTemplate = new(dialogueData, config);
        prompt = promptTemplate.TransformText();
        if (prompt == null)
          throw new Exception("Prompt is null.");
        int maxPromptLength = config.MaxPromptLength;
        if (prompt.Length > maxPromptLength)
        {
          wasTruncated = true;
          Console.WriteLine($"Prompt truncated to {maxPromptLength} characters. Original length was {prompt.Length} characters.");
          return prompt.Substring(0, maxPromptLength);
        }
        return prompt;
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred generating prompt.", ex);
        exception.Data.Add("dialogueData", JsonConvert.SerializeObject(dialogueData));
        throw exception;
      }
    }

    public static FloatData? GetFloatData(string data)
    {
      Match match = GetDataRegex.Match(data);
      if (match.Success)
      {
        string label = match.Groups[1].Value;
        float value = float.Parse(match.Groups[2].Value);
        return new FloatData(label, value);
      }
      return null;
    }



    public PromptTemplate(
      DialogueData dialogueData,
      Config tier)
    {
      DialogueData = dialogueData;
      Config = tier;
    }

    public DialogueData DialogueData { get; set; }

    public Config Config { get; set; }

    public string GetThoughts(string name, string[] thoughts)
    {
      return string.Join(", ", thoughts);
    }


    public int MaxOutputWords
    {
      get
      {
        if (this.DialogueData.maxWords > Config.MaxOutputWords)
          return Config.MaxOutputWords;
        else
          return this.DialogueData.maxWords;
      }
    }

    public string ColonyStartDaysAgoLabel
    {
      get
      {
        return TemplateHelper.DaysAgo(DialogueData.daysPassedSinceSettle);
      }
    }
  }
}
