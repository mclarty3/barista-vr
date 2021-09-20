using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beverage : MonoBehaviour
{
    public enum IngredientType 
    {
        Water,
        Milk,
        Espresso,
        Coffee

    }

    public struct IngredientAmount
    {
        public IngredientType ingredientType;
        public float amount;

        public IngredientAmount(IngredientType ingredientType, float amount)
        {
            this.ingredientType = ingredientType;
            this.amount = amount;
        }

    }

    public List<IngredientAmount> ingredients;

    public void AddIngredient(IngredientType ingredientType, float amount)
    {
        int index = ContainsIngredient(ingredientType);
        if (index != -1)
        {
            ingredients[index] = new IngredientAmount(ingredientType, 
                                                      ingredients[index].amount + amount);
        } else {
            ingredients.Add(new IngredientAmount(ingredientType, amount));
        }
    }

    public void RemoveIngredient(IngredientType ingredientType, float amount = -1)
    {
        int index = ContainsIngredient(ingredientType);
        if (index != -1)
        {
            if (amount == -1)
            {
                ingredients.RemoveAt(index);
            } else {
            ingredients[index] = new IngredientAmount(ingredientType,
                                                      ingredients[index].amount - amount);
            }
        }
    }

    public int ContainsIngredient(IngredientType ingredientType)
    {
        return ingredients.FindIndex(x => x.ingredientType == ingredientType);
    }

    public float GetIngredientAmount(IngredientType ingredientType)
    {
        int index = ContainsIngredient(ingredientType);
        if (index != -1)
        {
            return ingredients[index].amount;
        }
        return 0;
    }

    public float GetTotalAmount()
    {
        float total = 0;
        foreach (IngredientAmount ingredient in ingredients) {
            total += ingredient.amount;
        }
        return total;
    }

    public float GetIngredientPercentage(IngredientType ingredientType)
    {
        int index = ContainsIngredient(ingredientType);
        if (index != -1)
        {  
            return ingredients[index].amount / GetTotalAmount();
        }
        return 0;
    }

}
