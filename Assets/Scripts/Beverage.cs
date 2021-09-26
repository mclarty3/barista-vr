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
        Beverage beverage = BeverageList.list[Random.Range(0, BeverageList.list.Count)];

        if (beverage.name == "Latte" || beverage.name == "Steamed Milk" || beverage.name == "Cold milk")
        {
            if (Random.Range(0, 1) > 0.3) {
                Ingredient.IngredientType syrup = (Ingredient.IngredientType)Random.Range(
                    Ingredient.syrupIndices[0], Ingredient.syrupIndices[1] + 1
                );
                beverage.AddSyrup(syrup, Random.Range(1, 3));
            }
        }
        return beverage;
    }

    public void AddSyrup(Ingredient.IngredientType type, int numPumps)
    {
        float syrupRatio = BeverageList.PumpsToRatio(numPumps);
        foreach (KeyValuePair<Ingredient.IngredientType, float> entry in ingredientAmounts)
        {
            ingredientAmounts[entry.Key] -= entry.Value * (1 - syrupRatio);
        }
        ingredientAmounts.Add(type, syrupRatio);
    }

    public static float GetPercentError(float ideal, float actual)
    {
        return Mathf.Abs(ideal - actual) / ideal;
    }

    public float GetBeverageScore(ImprovedLiquid liquid, bool debug = false)
    {
        if (debug)
            Debug.Log("Comparing liquid to beverage " + name);
        float score = 0;
        // Check if liquid contains the right ratios of ingredients
        foreach (KeyValuePair<Ingredient.IngredientType, float> entry in ingredientAmounts)
        {
            float trueRatio = liquid.GetIngredientRatio(entry.Key);
            if (trueRatio == 0) {
                return 0;
            }
            float error = GetPercentError(entry.Value, trueRatio);
            if (error > 0.1) {
                float addScore = entry.Value * (1 - (error - 0.1f));
                if (debug)
                     Debug.Log("Adding " + addScore + " to " + name + " score for " + 
                               entry.Key + " error: " + error);
                score += addScore;
            } else if (debug) {
                Debug.Log("Not adding to " + name + " score for " + entry.Key + " error: " + error);
                score += entry.Value;
            }
        }
        // Check if liquid has any unknown ingredients
        foreach (KeyValuePair<Ingredient, float> entry in liquid.amounts)
        {
            if (!ingredientAmounts.ContainsKey(entry.Key.ingredientType)) {
                // Unknown ingredient in beverage
                score -= entry.Value;
                if (debug)
                    Debug.Log("Found " + entry.Key.ingredientType + " in liquid, but not in beverage, " +
                              "Deducting " + entry.Value + " from " + name + " score");
            }
        }
        // Check if liquid has enough volume
        if (liquid.lv.level < Beverage.idealLevel) {
            score -= (idealLevel - liquid.lv.level);
            if (debug) {
                Debug.Log("Deducting " + (idealLevel - liquid.lv.level) + " from " + name + " score " +
                          "for low liquid level");
            }
        }
        // Check if liquid has the right temperature
        if (Mathf.Abs(temperature - liquid.temperature) > 10) {
            float minusScore = (Mathf.Abs(temperature - liquid.temperature) - 10) * 0.01f;
            score -= minusScore;
            if (debug)
                Debug.Log("Deducting " + minusScore + " from " + name + " score for temperature error");
        }

        if (debug)
            Debug.Log("Final score for " + name + ": " + score);
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
}
