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

    public float GetBeverageScore(ImprovedLiquid liquid, bool debug = false)
    {
        float score = 0;
        // Check if liquid contains the right ratios of ingredients
        foreach (KeyValuePair<Ingredient.IngredientType, float> entry in ingredientAmounts)
        {
            float trueRatio = liquid.GetIngredientRatio(entry.Key);
            if (trueRatio == 0) {
                return 0;
            }
            float error = Mathf.Abs(trueRatio - entry.Value);
            if (error > 0.1) {
                float addScore = entry.Value * (1 - (error - 0.1f));
                score += addScore;
            }
        }
        // Check if liquid has any unknown ingredients
        foreach (KeyValuePair<Ingredient, float> entry in liquid.amounts)
        {
            if (!ingredientAmounts.ContainsKey(entry.Key.ingredientType)) {
                // Unknown ingredient in beverage
                score -= entry.Value;
            }
        }
        // Check if liquid has enough volume
        if (liquid.lv.level < Beverage.idealLevel) {
            score -= (idealLevel - liquid.lv.level);
        }
        // Check if liquid has the right temperature
        if (Mathf.Abs(temperature - liquid.temperature) > 10) {
            float minusScore = (Mathf.Abs(temperature - liquid.temperature) - 10) * 0.01f;
            score -= minusScore;
        }

        return score;
    }

    public float DebugBeverageScore(ImprovedLiquid liquid, out string debugString)
    {
        debugString = "";
        float score = 0;
        // Check if liquid contains the right ratios of ingredients
        foreach (KeyValuePair<Ingredient.IngredientType, float> entry in ingredientAmounts)
        {
            float trueRatio = liquid.GetIngredientRatio(entry.Key);
            if (trueRatio == 0) {
                debugString = "Missing ingredient " + entry.Key.ToString();
                debugString += "\nFinal score: 0 pts";
                return 0;
            }
            float error = Mathf.Abs(entry.Value - trueRatio);
            if (error > 0.1) {
                float addScore = entry.Value * (1 - error + 0.1f);
                debugString += (entry.Key.ToString() + " " +
                                System.Math.Round((1 - error + 0.1f) * 100, 2) +
                                "%: " + System.Math.Round(addScore * 100, 2) + " pts" + "\n");
                score += addScore;
            }  else {
                debugString += (entry.Key.ToString() + " 100%: " +
                                System.Math.Round(entry.Value * 100, 2) + " pts" + "\n");
                score += entry.Value;
            }
        }
        // Check if liquid has any unknown ingredients
        foreach (KeyValuePair<Ingredient, float> entry in liquid.amounts)
        {
            if (!ingredientAmounts.ContainsKey(entry.Key.ingredientType)) {
                // Unknown ingredient in beverage
                score -= entry.Value;
                debugString += ("Unknown ingredient: -" + System.Math.Round(entry.Value * 100, 2) +
                                " pts" + "\n");
            }
        }
        // Check if liquid has enough volume
        if (liquid.lv.level < Beverage.idealLevel) {
            score -= (idealLevel - liquid.lv.level);
            debugString += ("Insufficient volume: -" +
                            System.Math.Round((idealLevel - liquid.lv.level) * 100, 2) + " pts" + "\n");
        }
        // Check if liquid has the right temperature
        if (Mathf.Abs(temperature - liquid.temperature) > 10) {
            float minusScore = (Mathf.Abs(temperature - liquid.temperature) - 10) * 0.01f;
            score -= minusScore;
            debugString += ("Temperature: -" +
                            System.Math.Round(minusScore * 100, 2) + " pts" + "\n");
        }

        debugString += ("Score: " + System.Math.Round(score * 100, 2) + " pts" + "\n");
        return score;
    }

    public static Beverage IdentifyBeverage(ImprovedLiquid liquid)
    {
        foreach (Beverage beverage in BeverageList.list)
        {
            foreach (KeyValuePair<Ingredient.IngredientType, float> entry in beverage.ingredientAmounts)
            {
                if (liquid.GetIngredientRatio(entry.Key) == 0) {
                    continue;
                }
            }
            return beverage;
        }
        return null;
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
            if (error > 0.1) {
                float addScore = entry.Value * (1 - error + 0.1f);
                debugString += (entry.Key.ToString() + " " +
                                System.Math.Round((1 - error + 0.1f) * 100, 2) +
                                "%: " + System.Math.Round(addScore * 100, 2) + " pts" + "\n");
                score += addScore;
            }  else {
                debugString += (entry.Key.ToString() + " 100%: " +
                                System.Math.Round(entry.Value * 100, 2) + " pts" + "\n");
                score += entry.Value;
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
        if (fillAmountPercent < idealLevel) {
            var levelPenalty = System.Math.Abs(idealLevel - fillAmountPercent);
            score -= levelPenalty;
            debugString += "Insufficient volume: -" + System.Math.Round(levelPenalty * 100, 2) + " pts" + "\n";
        }
        // Check if liquid has the right temperature
        if (Mathf.Abs(temperature - ingredientManager.Temperature) > 10) {
            float minusScore = (Mathf.Abs(temperature - ingredientManager.Temperature) - 10) * 0.01f;
            score -= minusScore;
            debugString += "Temperature: -" + System.Math.Round(minusScore * 100, 2) + " pts" + "\n";
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
