using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiquidVolumeFX;

public class ImprovedLiquid : MonoBehaviour
{
    [Tooltip("Set the ingredient if the container will contain liquid on scene start")]
    [SerializeField]
    public Ingredient.IngredientType ingredientType = Ingredient.IngredientType.Undefined;
    public GameObject liquidDropPrefab;
    public int minDropsPerFrame = 5;
    public int maxDropsPerFrame = 15;
    public float pourPositionOffset = 0.05f;
    public float pourForceModifier = 1.5f;
    public float liquidScatter = 1f;
    public GameObject liquidSurfaceCollider;
    public float temperature;
    public bool SHOWINGREDIENTS = false;
    public bool QUERYBEVERAGETYPE = false;
    
    public LiquidVolume lv;
    public bool infiniteLiquid = false;

    private Vector3 spillPoint;
    public float meshVolume;
    private Dictionary<Color, float> colorRatio = new Dictionary<Color, float>();

    [SerializeField]
    public Dictionary<Ingredient, float> amounts = new Dictionary<Ingredient, float>();
    private GameManager gameManager;
    private float referenceVolume;
    private int numOzInReferenceVolume = 20;
    public int dropsPerOz = 50;

    // Start is called before the first frame update
    void Start()
    {
        if (!TryGetComponent<LiquidVolume>(out lv)) {
            Debug.LogError(name + " has ImprovedLiquid script but no LiquidVolume script.");
        }

        if (lv.level > 0) {
            Ingredient newIngredient = new Ingredient(ingredientType);
            amounts.Add(newIngredient, lv.level);
            temperature = newIngredient.temperature;
        }

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        referenceVolume = GetMeshVolume.VolumeOfMesh(
            gameManager.referenceLiquidVolume.gameObject.GetComponent<MeshFilter>().sharedMesh, 
            gameManager.referenceLiquidVolume.gameObject.transform);
        meshVolume = GetMeshVolume.VolumeOfMesh(lv.gameObject.GetComponent<MeshFilter>().sharedMesh, 
                                                lv.gameObject.transform);
        Debug.Log(transform.parent.name + " has " + meshVolume + " volume and " + 
                  GetVolumeInOz(meshVolume) + " oz with " + 
                  GetVolumeInOz(GetVolumeFilled()) + " oz filled");
    }

    // Update is called once per frame
    void Update()
    {
        if (liquidSurfaceCollider != null)
        {    
            Vector3 liquidPos = transform.position;
            liquidPos.y = lv.liquidSurfaceYPosition;
            liquidSurfaceCollider.transform.position = liquidPos;
            liquidSurfaceCollider.transform.rotation = Quaternion.Euler(
                0, liquidSurfaceCollider.transform.rotation.eulerAngles.y, 0);
        }
        if (SHOWINGREDIENTS) {
            DebugIngredients();
            SHOWINGREDIENTS = false;
        }
        if (QUERYBEVERAGETYPE) {
            Beverage beverage = Beverage.IdentifyBeverage(this);
            if (beverage != null) {
                beverage.GetBeverageScore(this, debug: true);
            }
            QUERYBEVERAGETYPE = false;
        }
    }

    void DebugIngredients() 
    {
        string ingredients = "";
        foreach (KeyValuePair<Ingredient, float> kvp in amounts) {
            ingredients += kvp.Key.ingredientType + ": " + kvp.Value + " | ";
        }
        Debug.Log(ingredients);
        Debug.Log(temperature + "°F");
    }

    void FixedUpdate()
    {
        Vector3 spillPos;
        float spillAmount;
        if (lv.GetSpillPoint(out spillPos, out spillAmount)) {
            Vector3 spillPos_local = transform.InverseTransformPoint(spillPos);
            Vector3 offsetVector = Vector3.ProjectOnPlane(transform.InverseTransformDirection(spillPos), 
                                                          transform.up).normalized;
            NewPour(spillPos + offsetVector * pourPositionOffset, spillAmount);
        }
    }

    bool ContainsIngredient(Ingredient.IngredientType ingredientType, out Ingredient ingredient)
    {
        foreach (KeyValuePair<Ingredient, float> kvp in amounts) {
            if (kvp.Key.ingredientType == ingredientType) {
                ingredient = kvp.Key;
                return true;
            }
        }
        ingredient = null;
        return false;
    }

    public void ChangeIngredient(Ingredient ingredient, Ingredient newIngredient)
    {
        foreach (KeyValuePair<Ingredient, float> kvp in amounts) {
            if (kvp.Key.ingredientType == ingredient.ingredientType) {
                float amount = kvp.Value;
                amounts.Remove(kvp.Key);
                amounts.Add(newIngredient, amount);
                return;
            }
        }
        Debug.Log("The ingredient " + ingredient.ingredientType + " was not found.");
    }

    public float GetIngredientRatio(Ingredient.IngredientType ingredientType)
    {
        float total = lv.level;
        foreach (KeyValuePair<Ingredient, float> kvp in amounts) {
            if (kvp.Key.ingredientType == ingredientType) {
                return kvp.Value / total;
            }
        }
        return 0;
    }
    
    private void NewPour(Vector3 spillPos, float spillAmount)
    {
        int drops = Mathf.FloorToInt(Mathf.Lerp(minDropsPerFrame, maxDropsPerFrame, spillAmount * 1.6f));
        LiquidSpout.PourLiquid(spillPos, spillPos - transform.position,
                               new List<Ingredient>(amounts.Keys), lv.liquidColor1, temperature, 1, 25, 
                               liquidDropPrefab, 0.05f * liquidScatter, 0.25f, 1.25f, pourForceModifier, 
                               drops: drops, randomForce: false);
        if (!infiniteLiquid)
        {
            float lostLevel = GetLevelFromDrops(drops);
            float lostPerIngredient = lostLevel / amounts.Count;
            Dictionary<Ingredient, float> temp = new Dictionary<Ingredient, float>();
            foreach (KeyValuePair<Ingredient, float> ingredientAmount in amounts)
            {
                float amount = ingredientAmount.Value - lostPerIngredient;
                if (amount >= 0.05f) {
                    temp.Add(ingredientAmount.Key, amount);
                } else {
                    lv.level -= ingredientAmount.Value;
                }
            }
            amounts = temp;
            lv.level -= lostLevel;
        }
    }


    public void AddDrop(DropBehavior drop) {
        Ingredient ingredient = drop.ingredient;
        AddLiquid(ingredient, GetLevelFromDrops());
    }

    public void AddLiquid(Ingredient ingredient, float amount) 
    {
        if (amount <= 0) return;

        if (ContainsIngredient(ingredient.ingredientType, out Ingredient existingIngredient)) {
            amounts[existingIngredient] += amount;
        } else {
            amounts.Add(ingredient, amount);
        }

        float newLevel = amount + lv.level;
        float weight = amount / newLevel;
        lv.liquidColor1 = HSBColor.Lerp(HSBColor.FromColor(lv.liquidColor1),
                                        HSBColor.FromColor(ingredient.color),
                                        amount / newLevel).ToColor();
        this.temperature = ((this.temperature * lv.level / newLevel) + 
                            (ingredient.temperature * amount / newLevel));
        lv.level = newLevel;
    }

    public float GetVolumeFilled()
    {
        return meshVolume * lv.level;
    }

    public float GetVolumeInOz(float volume)
    {
        return 20f * volume / referenceVolume;
    }

    public float GetVolumeFromDrops(int numDrops)
    {
        return (referenceVolume * numDrops) / (20 * dropsPerOz);
    }

    public float GetLevelFromDrops(int numDrops = 1)
    {
        return GetVolumeFromDrops(numDrops) / meshVolume;
    }

    public void SetMilkSteamed(bool milkSteamed)
    {
        ChangeIngredient(new Ingredient(Ingredient.IngredientType.Milk), 
                         new Ingredient(Ingredient.IngredientType.SteamedMilk));
    }
}
