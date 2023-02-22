using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beverage
{
    public string name;
    public Dictionary<Ingredient.IngredientType, float> ingredientAmounts;
    public float temperature;
    public static float idealLevel = 0.85f;

    /// <summary>
    /// Creates a Beverage object, with a given name, ideal temperature,
    /// and a dictionary mapping ingredients to what ratio they should be in the beverage.<br/>
    /// E.g: "Latte", 150, {Espresso: 0.15, SteamedMilk: 0.85}
    /// </summary>
    public Beverage(string name, float temperature,
                    Dictionary<Ingredient.IngredientType, float> ingredientAmounts)
    {
        this.name = name;
        this.ingredientAmounts = ingredientAmounts;
        this.temperature = temperature;
    }

    public static Beverage GenerateBeverage()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        Beverage randomBev = BeverageList.list[Random.Range(0, BeverageList.list.Count)];
        Beverage beverage = new Beverage(randomBev.name, randomBev.temperature,
                                         new Dictionary<Ingredient.IngredientType, float>(
                                             randomBev.ingredientAmounts)
                                         );

        if (beverage.name == "Latte" || beverage.name == "Steamed Milk" || beverage.name == "Cold milk")
        {
            if (Random.Range(0, 1.0f) > 0.3) {
                int rand = Random.Range(Ingredient.syrupIndices[0], Ingredient.syrupIndices[1] + 1);
                Debug.Log(rand);
                Ingredient.IngredientType syrup = (Ingredient.IngredientType)rand;
                beverage.AddSyrup(syrup, 3);
            }
        }
        return beverage;
    }

    public void AddSyrup(Ingredient.IngredientType type, int numPumps)
    {
        float syrupRatio = BeverageList.PumpsToRatio(numPumps);
        var temp = new Dictionary<Ingredient.IngredientType, float>();
        foreach (KeyValuePair<Ingredient.IngredientType, float> entry in ingredientAmounts)
        {
            temp.Add(entry.Key, entry.Value * (1 - syrupRatio));
            // ingredientAmounts[entry.Key] -= entry.Value * (1 - syrupRatio);
        }
        // ingredientAmounts.Add(type, syrupRatio);
        temp.Add(type, syrupRatio);
        ingredientAmounts = temp;
        name = type.ToString().Remove(type.ToString().IndexOf("Syrup")) + " " + name;
    }

    public float DebugBeverageScore(IngredientManager ingredientManager, out string debugString)
    {
        debugString = "";
        float score = 0;

        // Check if liquid contains the right ratios of ingredients
        foreach (KeyValuePair<Ingredient.IngredientType, float> entry in ingredientAmounts)
        {
            float trueRatio = ingredientManager.GetIngredientRatio(entry.Key);
            if (trueRatio == 0) {
                debugString = "Missing ingredient " + entry.Key.ToString();
                debugString += "\nFinal score: 0 pts";
                return 0;
            }
            float error = Mathf.Abs(entry.Value - trueRatio);
            float pctError = error / entry.Value;
            if (pctError > 0.1) {
                float addScore = entry.Value * (1 - pctError);
                score += addScore;
                debugString += entry.Key.ToString() + " (" + trueRatio + "/" + entry.Value + ") ";
                debugString += System.Math.Round((1 - pctError) * 100, 2) + "%: ";
                debugString += "%: -" + System.Math.Round(addScore * 100, 2) + " pts" + "\n";
            }  else {
                score += entry.Value;
                debugString += entry.Key.ToString() + " 100%: " + System.Math.Round(entry.Value * 100, 2) + " pts" + "\n";
            }
        }

        // Check if liquid has any unknown ingredients
        foreach (KeyValuePair<Ingredient.IngredientType, float> entry in ingredientManager.IngredientAmounts)
        {
            if (!ingredientAmounts.ContainsKey(entry.Key)) {
                // Unknown ingredient in beverage
                score -= entry.Value;
                debugString += "Unknown ingredient: -" + System.Math.Round(entry.Value * 100, 2) + " pts\n";
            }
        }

        // Check if liquid has enough volume
        var fillAmountPercent = ingredientManager.liquidContainer.FillAmountPercent;
        var volumeDifference = Mathf.Max(5, Mathf.Abs(idealLevel - fillAmountPercent)) - 5;

        if (volumeDifference > 0) {
            var levelPenalty = System.Math.Abs(idealLevel - fillAmountPercent);
            score -= levelPenalty;
            debugString += "Insufficient volume: -" + System.Math.Round(levelPenalty * 100, 2) + " pts" + "\n";
        }

        // Check if liquid has the right temperature
        var temperatureDifference = Mathf.Max(5, Mathf.Abs(temperature - ingredientManager.Temperature)) - 5;
        if (temperatureDifference > 0) {
            score -= temperatureDifference / 100;
            debugString += "Temperature (" + ingredientManager.Temperature + "/" + temperature + "): ";
            debugString += "-" + System.Math.Round(temperatureDifference, 2) + " pts" + "\n";
        }

        debugString += "Score: " + System.Math.Round(score * 100, 2) + " pts" + "\n";
        return score;
    }

    public static Beverage IdentifyBeverage(IngredientManager ingredientManager)
    {
        foreach (Beverage beverage in BeverageList.list)
        {
            foreach (KeyValuePair<Ingredient.IngredientType, float> entry in beverage.ingredientAmounts)
            {
                if (ingredientManager.GetIngredientRatio(entry.Key) == 0) {
                    continue;
                }
            }
            return beverage;
        }
        return null;
    }
}
