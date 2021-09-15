using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient
{

    public enum IngredientType
    {
        Milk,
        Espresso,
        VanillaSyrup,
        HazlenutSyrup,
        CaramelSyrup,
        ChocolateSyrup,
        WhiteChocolateSyrup,
        CinnamonSyrup,
        PeppermintSyrup
    }

    public IngredientType ingredientType;
    public float temperature;

    public Ingredient(IngredientType type, float temp)
    {
        ingredientType = type;
        temperature = temp;
    }
}
