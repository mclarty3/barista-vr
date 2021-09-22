using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beverage : MonoBehaviour
{

    public class IngredientAmount
    {
        public Ingredient.IngredientType ingredientType;
        public float amount;

        public IngredientAmount()
        {
            ingredientType = Ingredient.IngredientType.Undefined;
            amount = 0;
        }
        public IngredientAmount(Ingredient.IngredientType ingredientType, float amount)
        {
            this.ingredientType = ingredientType;
            this.amount = amount;
        }
    }

    public List<IngredientAmount> ingredients;
    public Dictionary<Ingredient.IngredientType, float> ingredientAmounts;

    public void AddIngredient(Ingredient.IngredientType ingredientType, float amount)
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

    public void RemoveIngredient(Ingredient.IngredientType ingredientType, float amount = -1)
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

    public int ContainsIngredient(Ingredient.IngredientType ingredientType)
    {
        return ingredients.FindIndex(x => x.ingredientType == ingredientType);
    }

    public float GetIngredientAmount(Ingredient.IngredientType ingredientType)
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

    public float GetIngredientPercentage(Ingredient.IngredientType ingredientType)
    {
        int index = ContainsIngredient(ingredientType);
        if (index != -1)
        {  
            return ingredients[index].amount / GetTotalAmount();
        }
        return 0;
    }

}
