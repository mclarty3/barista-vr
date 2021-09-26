using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ingredient
{


    [System.Serializable]
    public enum IngredientType
    {
        Undefined,
        Milk,
        SteamedMilk,
        Espresso,
        VanillaSyrup,
        HazlenutSyrup,
        CaramelSyrup,
        ChocolateSyrup,
        WhiteChocolateSyrup,
        CinnamonSyrup,
        PeppermintSyrup
    }

    public static int[] syrupIndices = {4, 6};

    public Color32[] ingredientColors = new Color32[]
    {
        Color.black,
        Color.white,
        Color.white,
        new Color32(44, 21, 0, 255),
        new Color32(173, 129, 45, 255),
        new Color32(173, 129, 45, 255),
        new Color32(255, 255, 255, 255),
        new Color32(255, 255, 255, 255),
        new Color32(255, 255, 255, 255),
        new Color32(255, 255, 255, 255),
        new Color32(255, 255, 255, 255),
    };

    public float[] ingredientTemps = new float[]
    {
        0.0f,
        36,
        145,
        160,
        65,
        65,
        65,
        65,
        65,
        65,
        65,
    };

    public IngredientType ingredientType;
    public Color color;
    public float temperature;

    public Ingredient()
    {
        ingredientType = IngredientType.Undefined;
        color = Color.white;
        temperature = 0.0f;
    }

    public Ingredient(IngredientType type, Color? color = null, float temperature = -1)
    {
        ingredientType = type;
        this.color = (Color)(color == null ? ingredientColors[(int)type] : color);
        this.temperature = temperature == -1 ? ingredientTemps[(int)type] : temperature; 
    }
}
