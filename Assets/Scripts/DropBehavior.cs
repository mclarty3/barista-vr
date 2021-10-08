using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropBehavior : MonoBehaviour
{
    [HideInInspector]
    public Color dropColor;

    [Tooltip("How many seconds pass before the drop despawns")]
    public int dropLife;

    public Ingredient ingredient;

    private float timeCreated;

    // Start is called before the first frame update
    void Start()
    {
        ingredient = new Ingredient(Ingredient.IngredientType.Milk);
        dropColor = this.GetComponent<Renderer>().material.color;
        timeCreated = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - timeCreated >= dropLife) {
            Destroy(this.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("TriggerEnter");
        if (other.gameObject.tag != "LiquidCatcher") {
            Destroy(gameObject);
            // Debug.Log("Destroyed");
        }
        else {
            // Debug.Log("Adding liquid for 1 drop");
            Destroy(gameObject);
            LiquidCatcher temp;
            if (other.TryGetComponent<LiquidCatcher>(out temp))
                temp.OnCatchLiquid(this);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        GameObject otherObj = other.collider.gameObject;
        LiquidCatcher temp;
        if (otherObj.tag == "LiquidCatcher") {
            if (otherObj.TryGetComponent<LiquidCatcher>(out temp))
                temp.OnCatchLiquid(this);
        } else if (otherObj.transform.parent != null && 
                   otherObj.transform.parent.parent != null &&
                   otherObj.transform.parent.parent.tag == "LiquidCatcher") {
            if (otherObj.transform.parent.parent.TryGetComponent<LiquidCatcher>(out temp))
                temp.OnCatchLiquid(this);
        }
        Destroy(gameObject);
    }
}
