using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeverageList 
{
    static public System.Func<float, float> PumpsToRatio = pumps => pumps * 0.02f;

    static public List<Beverage> list = new List<Beverage>()
    {
        new Beverage("Latte", 150, new Dictionary<Ingredient.IngredientType, float>()
            {
                {Ingredient.IngredientType.Espresso, 0.25f},
                {Ingredient.IngredientType.SteamedMilk, 0.75f},
            }
        ),
        new Beverage("Steamed Milk", 145, new Dictionary<Ingredient.IngredientType, float>()
            {
                {Ingredient.IngredientType.Milk, 1}
            }
        ),
        new Beverage("Cold Milk", 40, new Dictionary<Ingredient.IngredientType, float>()
            {
                {Ingredient.IngredientType.Milk, 1}
            }
        )
    };
}
