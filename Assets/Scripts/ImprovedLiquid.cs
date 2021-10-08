using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LiquidVolumeFX;

public class ImprovedLiquid : MonoBehaviour
{
    [Header("Old drop stuff")]
    public GameObject liquidDropPrefab;
    public int minDropsPerFrame = 5;
    public int maxDropsPerFrame = 15;
    public float pourForceModifier = 1.5f;
    public float liquidScatter = 1f;
    [Header("Pour settings")]
    [Tooltip("The minimum number of drops (particles) that will be emitted per frame when pouring")]
    public int _minDropsPerFrame = 100;
    [Tooltip("The maximum number of drops (particles) that will be emitted per frame when pouring")]
    public int _maxDropsPerFrame = 10000;
    [Tooltip("How many drops are emitted for each ounce of liquid")]
    public int pouredDropsPerOz = 300;
    [Tooltip("How many drops caught by the liquidCatcher correspond to one ounce of liquid")]
    public int receivedDropsPerOz = 42;
    [Tooltip("Determines how far from the pour point the liquid pours, parallel to container up/down")]
    public float pourVerticalOffset = 0.05f;
    [Tooltip("Determines how far from the pour point the liquid pours, parallel to container forward")]
    public float pourHorizontalOffset = 0.05f;
    [Tooltip("The surface collider for the milk pitcher, to detect collision with the steam wand")]
    public GameObject liquidSurfaceCollider;
    [Header("Beverage debugging")]
    [Tooltip("Set the ingredient if the container will contain liquid on scene start")]
    public bool infiniteLiquid = false;
    [SerializeField]
    public Ingredient.IngredientType ingredientType = Ingredient.IngredientType.Undefined;
    public float temperature;
    public bool SHOWINGREDIENTS = false;
    public bool QUERYBEVERAGETYPE = false;
    public bool useNewPouringSystem = false;
    
    public Dictionary<Ingredient, float> amounts = new Dictionary<Ingredient, float>();
    [System.NonSerialized]
    public LiquidVolume lv;
    
    private float meshVolume;
    private float referenceVolume;
    private ParticleManager particleManager = null;
    private GameManager gameManager;
    private bool empty = false;

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
        // Debug.Log(transform.parent.name + " has " + meshVolume + " volume and " + 
        //           GetVolumeInOz(meshVolume) + " oz with " + 
        //           GetVolumeInOz(GetVolumeFilled()) + " oz filled");

        particleManager = transform.parent.GetComponentInChildren<ParticleManager>();
        if (particleManager != null) {
            minDropsPerFrame = _minDropsPerFrame;
            maxDropsPerFrame = _maxDropsPerFrame;
        }
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

        if (!empty && lv.level <= 0.01f) {
            empty = true;
            lv.level = 0;
            Color c = lv.liquidColor1;
            Color invis = new Color(c.r, c.g, c.b, 0);
            lv.liquidColor1 = invis;
        } else if (empty && lv.level > 0.01f) {
            empty = false;
            Color c = lv.liquidColor1;
            Color vis = new Color(c.r, c.g, c.b, 1);
            lv.liquidColor1 = vis;
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
                Vector3 offsetVector = Vector3.ProjectOnPlane(spillPos - transform.position,
                                                            transform.up).normalized;
                Vector3 finalPos = (spillPos + (offsetVector * pourHorizontalOffset) +
                                    (transform.up * pourVerticalOffset));
            if (particleManager == null) {
                NewPour(finalPos, spillAmount);
            } else {
                ParticlePour(finalPos, spillAmount);
            }
        } else if (particleManager != null && particleManager.isPouring) {
            particleManager.StopPouring();
        }
    }

    void ParticlePour(Vector3 position, float spillAmount) 
    {
        particleManager.particleFlowRate = (int)Mathf.Lerp(minDropsPerFrame, maxDropsPerFrame,
                                                           spillAmount);
        particleManager.transform.position = position;
        Vector3 dir = position - transform.position;
        particleManager.transform.rotation = Quaternion.LookRotation(dir);
        particleManager.StartPouring(amounts);
    }

    public void ReduceLiquid(int lostDrops)
    {
        if (!infiniteLiquid)
        {
            float lostLevel = GetLevelFromDrops(lostDrops);
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

    public void AddDrop(Ingredient ingredient) {
        float level = GetLevelFromDrops(pourReceive: -1);
        AddLiquid(ingredient, level);
        Debug.Log("adding level: " + level);
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

    public float GetVolumeFromDrops(int numDrops, int pourReceive = 1)
    {
        if (pourReceive == 1) {
            return (referenceVolume * numDrops) / (20 * pouredDropsPerOz);
        } else if (pourReceive == -1) {
            return (referenceVolume * numDrops) / (20 * receivedDropsPerOz);
        } else {
            Debug.LogError("Error: pourReceive must be either -1 or 1. Got " + pourReceive);
            return 0;
        }
    }

    public float GetLevelFromDrops(int numDrops = 1, int pourReceive = 1)
    {
        return GetVolumeFromDrops(numDrops, pourReceive) / meshVolume;
    }

    public void SetMilkSteamed(bool milkSteamed)
    {
        ChangeIngredient(new Ingredient(Ingredient.IngredientType.Milk), 
                         new Ingredient(Ingredient.IngredientType.SteamedMilk));
    }
}
