namespace RimDialogueObjects.Templates
{
  public static class TemplateHelper
  {
    public static string DescribeOutputLength(int outputWords)
    {
      if (outputWords <= 25)
        return "an extremely brief";
      else if (outputWords <= 35)
        return "a brief";
      else if (outputWords <= 50)
        return "a short";
      else if (outputWords <= 80)
        return "a compact";
      else
        return "a";
    }
    public static string DescribeDefenses(int defenseCount)
    {
      if (defenseCount < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(defenseCount), "Defense count must be greater than or equal to 0");
      }

      if (defenseCount == 0)
        return "no defenses at all.";
      else if (defenseCount <= 5)
        return "minimal defenses.";
      else if (defenseCount <= 10)
        return "a few defenses.";
      else if (defenseCount <= 20)
        return "some basic defenses.";
      else if (defenseCount <= 40)
        return "moderate defenses.";
      else if (defenseCount <= 80)
        return "strong defenses.";
      else if (defenseCount <= 160)
        return "very strong defenses.";
      else
        return "an impenetrable wall of defenses.";
    }

    public static string TemperatureFeel(float temperatureCelsius)
    {
      if (temperatureCelsius <= -30.0f)
        return "freezing";
      else if (temperatureCelsius <= -10.0f)
        return "bitter cold";
      else if (temperatureCelsius <= 0.0f)
        return "very cold";
      else if (temperatureCelsius <= 10.0f)
        return "chilly";
      else if (temperatureCelsius <= 15.0f)
        return "cool";
      else if (temperatureCelsius <= 20.0f)
        return "mild";
      else if (temperatureCelsius <= 25.0f)
        return "comfortable";
      else if (temperatureCelsius <= 30.0f)
        return "warm";
      else if (temperatureCelsius <= 35.0f)
        return "hot";
      else if (temperatureCelsius <= 40.0f)
        return "very hot";
      else
        return "scorching";
    }

    public static string DaysAgo(int numberOfDaysAgo)
    {
      if (numberOfDaysAgo < 0)
        throw new ArgumentOutOfRangeException(nameof(numberOfDaysAgo), "The number of days cannot be negative.");

      if (numberOfDaysAgo == 0)
        return "today";
      else if (numberOfDaysAgo == 1)
        return "yesterday";
      else if (numberOfDaysAgo <= 2)
        return "a couple of days ago";
      else if (numberOfDaysAgo <= 3)
        return "a few days ago";
      else if (numberOfDaysAgo <= 4)
        return "about a week ago";
      else if (numberOfDaysAgo <= 7)
        return "a couple of weeks ago";
      else if (numberOfDaysAgo <= 15)
        return "about a month ago";
      else if (numberOfDaysAgo <= 30)
        return "a month or two ago";
      else if (numberOfDaysAgo <= 45)
        return "a few months ago";
      else if (numberOfDaysAgo <= 60)
        return "about a year ago";
      else if (numberOfDaysAgo <= 120)
        return "over a year ago";
      else if (numberOfDaysAgo <= 180)
        return "a couple of years ago";
      else if (numberOfDaysAgo <= 300)
        return "several years back";
      else
        return "a very long time ago";
    }

    public static string WealthDescription(float wealth)
    {
      if (wealth < 0)
        throw new ArgumentOutOfRangeException(nameof(wealth), "Wealth must be greater than 0.");

      if (wealth == 0)
        return "The colony is destitute, barely scraping by with no resources.";
      else if (wealth <= 5_000)
        return "The colony is very poor, with only the most basic resources to survive.";
      else if (wealth <= 20_000)
        return "The colony is poor, struggling to meet basic needs.";
      else if (wealth <= 50_000)
        return "The colony has modest resources, enough to sustain a simple lifestyle.";
      else if (wealth <= 100_000)
        return "The colony is getting by, with enough resources to be comfortable, but nothing luxurious.";
      else if (wealth <= 200_000)
        return "The colony is comfortable, with decent resources and a sense of security.";
      else if (wealth <= 400_000)
        return "The colony is well-off, with plentiful resources and ample supplies.";
      else if (wealth <= 600_000)
        return "The colony is affluent, with a wealth of resources and an enviable lifestyle.";
      else if (wealth <= 800_000)
        return "The colony is wealthy, with substantial assets and high standards of living.";
      else if (wealth <= 900_000)
        return "The colony is very wealthy, with extensive resources and luxurious amenities.";
      else
        return "The colony is a powerhouse, reaching millionaire status with a vast wealth of resources.";
    }

    public static string DescribeTimeOfDay(int hourOfDay)
    {
      if (hourOfDay < 0 || hourOfDay > 24)
        throw new ArgumentOutOfRangeException(nameof(hourOfDay), "Hour of day must be between 0 and 24.");

      if (hourOfDay < 3)
        return "late night";
      else if (hourOfDay < 6)
        return "early morning";
      else if (hourOfDay < 9)
        return "morning";
      else if (hourOfDay < 12)
        return "late morning";
      else if (hourOfDay < 14)
        return "midday";
      else if (hourOfDay < 16)
        return "early afternoon";
      else if (hourOfDay < 18)
        return "afternoon";
      else if (hourOfDay < 20)
        return "early evening";
      else if (hourOfDay < 22)
        return "evening";
      else if (hourOfDay < 24)
        return "late evening";
      else
        return "midnight";
    }

    public static string DescribeFoodAmount(float foodTotal, int colonistCount, int prisonerCount)
    {
      if (colonistCount + prisonerCount < 1)
        throw new ArgumentOutOfRangeException(nameof(colonistCount), "Colonist count plus prisoner count must be greater than or equal to 1.");

      if (foodTotal < 0)
        throw new ArgumentOutOfRangeException(nameof(foodTotal), "Food Total must be greater than or equal to 0.");

      float foodAmount = foodTotal / (colonistCount + (float)prisonerCount);

      if (foodAmount == 0f)
        return "no food at all.";
      else if (foodAmount < 5f)
        return "barely any food.";
      else if (foodAmount < 10f)
        return "a small amount of food.";
      else if (foodAmount < 20f)
        return "a moderate supply of food.";
      else if (foodAmount < 30f)
        return "a good amount of food.";
      else if (foodAmount < 40f)
        return "a lot of food.";
      else if (foodAmount < 50f)
        return "almost overflowing with food.";
      else
        return "an enormous stockpile of food.";
    }

    public static string DescribeImpressiveness(float impressiveness)
    {
      if (impressiveness < 20)
        return "awful";
      else if (impressiveness < 30)
        return "dull";
      else if (impressiveness < 40)
        return "mediocre";
      else if (impressiveness < 50)
        return "decent";
      else if (impressiveness < 65)
        return "slightly impressive";
      else if (impressiveness < 85)
        return "somewhat impressive";
      else if (impressiveness < 120)
        return "very impressive";
      else if (impressiveness < 170)
        return "extremely impressive";
      else if (impressiveness < 240)
        return "unbelievably impressive";
      else
        return "wondrously impressive";
    }
    public static string DescribeCleanliness(float cleanliness)
    {
      if (cleanliness < -1.1f)
        return "very dirty";
      else if (cleanliness < -0.4f)
        return "dirty";
      else if (cleanliness < -0.05f)
        return "slightly dirty";
      else if (cleanliness < 0.4f)
        return "clean";
      else
        return "sterile";
    }

    public static string DescribeOpinion(float opinion)
    {
      if (opinion < -110)
        return "utterly despises";
      else if (opinion < -90)
        return "loathes";
      else if (opinion < -70)
        return "detests";
      else if (opinion < -50)
        return "hates";
      else if (opinion < -30)
        return "strongly dislikes";
      else if (opinion < -15)
        return "dislikes";
      else if (opinion < -5)
        return "has mildly negative feelings toward";
      else if (opinion < 5)
        return "is neutral toward";
      else if (opinion < 15)
        return "has mild positive feelings toward";
      else if (opinion < 30)
        return "likes";
      else if (opinion < 50)
        return "strongly likes";
      else if (opinion < 70)
        return "admires";
      else if (opinion < 90)
        return "greatly admires";
      else if (opinion < 110)
        return "loves";
      else
        return "deeply loves";
    }
    public static string DescribeDrugSupply(int drugsCount, int colonistCount)
    {
      if (colonistCount <= 0)
        throw new ArgumentOutOfRangeException(nameof(colonistCount), "Colonist count must be greater than 0.");
      if (drugsCount < 0)
        throw new ArgumentOutOfRangeException(nameof(drugsCount), "Drugs count cannot be negative.");

      float drugsPerColonist = (float)drugsCount / colonistCount;

      if (drugsPerColonist == 0)
        return "no drugs at all.";
      else if (drugsPerColonist < 1.0f)
        return "almost no drugs.";
      else if (drugsPerColonist < 5.0f)
        return "a small supply of drugs.";
      else if (drugsPerColonist < 10.0f)
        return "a moderate supply of drugs.";
      else if (drugsPerColonist < 20.0f)
        return "a good surplus of drugs.";
      else if (drugsPerColonist < 30.0f)
        return "a large stockpile of drugs.";
      else
        return "an overwhelming amount of drugs.";
    }

    public static string DescribeMedicineSupply(int medicineCount)
    {
      if (medicineCount < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(medicineCount), "Medicine count must be greater than or equal to 0.");
      }

      if (medicineCount == 0)
        return "no medicine.";
      else if (medicineCount <= 5)
        return "almost no medicine.";
      else if (medicineCount <= 10)
        return "a small supply of medicine.";
      else if (medicineCount <= 20)
        return "a modest amount of medicine.";
      else if (medicineCount <= 40)
        return "a decent supply of medicine.";
      else if (medicineCount <= 80)
        return "a large amount of medicine.";
      else if (medicineCount <= 160)
        return "a very large supply of medicine.";
      else
        return "a huge amount of medicine.";
    }


    public static string DescribeColonySize(int colonistCount)
    {
      if (colonistCount < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(colonistCount), "Colonist count must be between greater than or equal to 0.");
      }

      if (colonistCount <= 3)
        return "a tiny colony.";
      else if (colonistCount <= 6)
        return "a small colony.";
      else if (colonistCount <= 10)
        return "a modest-sized colony.";
      else if (colonistCount <= 20)
        return "a large colony.";
      else if (colonistCount <= 40)
        return "a very large colony.";
      else if (colonistCount <= 80)
        return "a massive colony.";
      else
        return "an enormous colony.";
    }

    public static string DescribePrisonerCount(int prisonerCount)
    {
      if (prisonerCount <= 0)
        return "no prisoners.";
      else if (prisonerCount <= 2)
        return "a couple of prisoners.";
      else if (prisonerCount <= 5)
        return "a small number of prisoners.";
      else if (prisonerCount <= 10)
        return "a modest group of prisoners.";
      else if (prisonerCount <= 15)
        return "a significant number of prisoners.";
      else if (prisonerCount <= 20)
        return "a large group of prisoners.";
      else if (prisonerCount <= 25)
        return "a very large group of prisoners.";
      else
        return "a huge amount of prisoners.";
    }

    public static string DescribeComfortLevel(float comfort)
    {
      if (comfort <= 0)
        return "very uncomfortable";
      else if (comfort < 0.15f)
        return "uncomfortable";
      else if (comfort < 0.3f)
        return "somewhat uncomfortable";
      else if (comfort < 0.5f)
        return "a little uncomfortable";
      else if (comfort < 0.7f)
        return "somewhat comfortable";
      else if (comfort < 0.85f)
        return "comfortable";
      else if (comfort < 1f)
        return "very comfortable";
      else // comfort == 1
        return "perfectly comfortable";
    }

    public static string DescribeHungerLevel(float hunger)
    {
      if (hunger <= 0)
        return "starving";
      else if (hunger < 0.15f)
        return "very hungry";
      else if (hunger < 0.3f)
        return "hungry";
      else if (hunger < 0.5f)
        return "peckish";
      else if (hunger < 0.7f)
        return "not very hungry";
      else if (hunger < 0.85f)
        return "full";
      else if (hunger < 1f)
        return "very full";
      else
        return "stuffed";
    }

    public static string DescribeRestLevel(float restLevel)
    {
      if (restLevel <= 0)
        return "exhausted";
      else if (restLevel < 0.15f)
        return "very tired";
      else if (restLevel < 0.3f)
        return "tired";
      else if (restLevel < 0.5f)
        return "drowsy";
      else if (restLevel < 0.7f)
        return "somewhat rested";
      else if (restLevel < 0.85f)
        return "rested";
      else if (restLevel < 1f)
        return "very well rested";
      else
        return "completely refreshed";
    }

    public static string DescribeEngagementLevel(float engagementLevel)
    {
      if (engagementLevel <= 0)
        return "completely bored";
      else if (engagementLevel < 0.15f)
        return "very bored";
      else if (engagementLevel < 0.3f)
        return "bored";
      else if (engagementLevel < 0.5f)
        return "somewhat uninterested";
      else if (engagementLevel < 0.7f)
        return "moderately engaged";
      else if (engagementLevel < 0.85f)
        return "engaged";
      else if (engagementLevel < 1f)
        return "very interested";
      else
        return "completely enthralled";
    }

    public static string DescribeEnvironmentBeauty(float beautyLevel)
    {
      if (beautyLevel <= 0)
        return "absolutely hideous";
      else if (beautyLevel < 0.15f)
        return "very ugly";
      else if (beautyLevel < 0.3f)
        return "ugly";
      else if (beautyLevel < 0.5f)
        return "plain and uninspiring";
      else if (beautyLevel < 0.7f)
        return "somewhat pleasant";
      else if (beautyLevel < 0.85f)
        return "attractive";
      else if (beautyLevel < 1f)
        return "beautiful";
      else
        return "breathtakingly stunning";
    }

    public static string PercentToLabel(float value)
    {
      if (value == 0)
        return "none";
      else if (value <= 0.1)
        return "extremely low";
      else if (value <= 0.2)
        return "very low";
      else if (value <= 0.3)
        return "low";
      else if (value <= 0.4)
        return "below average";
      else if (value <= 0.5)
        return "moderate";
      else if (value <= 0.6)
        return "normal";
      else if (value <= 0.7)
        return "above average";
      else if (value <= 0.8)
        return "high";
      else if (value <= 0.9)
        return "very high";
      else if (value < 1)
        return "extremely high";
      else if (value == 1)
        return "maximum";
      else
        return "unknown";
    }

  }
}
