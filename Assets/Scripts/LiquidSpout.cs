using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidSpout : MonoBehaviour
{
    public GameObject dropPrefab;
    public Ingredient ingredient;
    public Color dropColour;
    public float liquidTemperature = 0.0f;
    public bool angleDependent = true;
    public float angleThreshold = 90f;
    public bool active = false;
    public int drops = 10;
    public int dropsPerSecond = -1;
    [SerializeField]

    [Header("Pour settings")]
    [Tooltip("The minimum number of drops that will be instantiated each FixedUpdate")]
    public int minDropsPerFrame = 1;
    [Tooltip("The maximum number of drops that will be instantiated each FixedUpdate")]
    public int maxDropsPerFrame = 25;
    [Tooltip("Determines random drop position variations within a unit sphere surrounding the spout")]
    public float dropPositionOffset = 0.05f;
    [Tooltip("The minimum multiplier for random drop size variations")]
    public float dropMinScaleMultiplier = 0.25f;
    [Tooltip("The maximum multiplier for random drop size variations")]
    public float dropMaxScaleMultiplier = 1.25f;
    [Tooltip("Determines how heavily the drops are pushed towards the up vector of the spout")]
    public float pourForceMultipilier = 2;
    [Tooltip("Whether the drops will undergo a small random force upon being instantiated")]
    public bool randomDropForce = true;

    private Vector3 liquidOutVect;
    private float lastDropTime;


    // Start is called before the first frame update
    void Start()
    {
        if (ingredient.ingredientType == Ingredient.IngredientType.Undefined)
        {
            throw new System.Exception("LiquidSpout must have an ingredient defined");
        }
        ingredient = new Ingredient(ingredient.ingredientType);
        dropColour = ingredient.color;
        liquidTemperature = ingredient.temperature;
        liquidOutVect = this.gameObject.transform.up;
        lastDropTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        liquidOutVect = this.gameObject.transform.up;
    }

    void FixedUpdate()
    {
        if (angleDependent) {
            float angle = Vector3.Angle(Vector3.up, liquidOutVect);
            if (angle > angleThreshold) {
                PourLiquid(angle: angle, randomForce: randomDropForce);
            }
        } else if (active) {
            if (dropsPerSecond == -1) {
                PourLiquid(drops, modifier: 1, randomForce: randomDropForce);
            } else {
                if (Time.time - lastDropTime > (1.0f / dropsPerSecond)) {
                    PourLiquid(drops: 1, modifier: 1, randomForce: randomDropForce);
                    lastDropTime = Time.time;
                }
            }
        }
    }

    void PourLiquid(int drops = -1, float modifier = -1, float angle = -1, float force_modifier = 1,
                    bool randomForce = true) {
        List<Ingredient> ingredients = new List<Ingredient>() { this.ingredient };
        LiquidSpout.PourLiquid(transform.position, 
                               transform.up.normalized,
                               ingredients, dropColour, liquidTemperature, minDropsPerFrame, 
                               maxDropsPerFrame, dropPrefab, dropPositionOffset,dropMinScaleMultiplier, 
                               dropMaxScaleMultiplier, pourForceMultipilier, drops, 
                               modifier, angle, angleThreshold, randomForce);
    }

    public static void PourLiquid(Vector3 pourPoint, Vector3 pourDirection, List<Ingredient> ingredients,
                                  Color color, float temperature, float minDropsPerFrame, 
                                  float maxDropsPerFrame, GameObject dropPrefab, 
                                  float dropPositionOffset,float dropMinScaleMultiplier, 
                                  float dropMaxScaleMultiplier,float pourForceMultipilier, 
                                  int drops=-1, float modifier=-1, float angle=-1, 
                                  float angleThreshold=90, bool randomForce = true) {
        if (modifier == -1) {
            modifier = angle == -1 ? Random.value : (angle - angleThreshold) / (180 - angleThreshold);
        }
        if (drops == -1) {
            drops = Mathf.FloorToInt(Mathf.Lerp(minDropsPerFrame, maxDropsPerFrame, modifier));
        }

        for (int i = 0; i < drops; i++) {
            // Designed to iterate equally over all ingredients
            Ingredient ingredient = new Ingredient(Ingredient.IngredientType.Undefined);
            try {
                ingredient = ingredients[i % ingredients.Count];
            } catch (System.DivideByZeroException) {
                Debug.Log("No ingredients in liquid. Did you artifically raise the liquid level?");
            }
            ingredient.temperature = temperature;
            GameObject oneSpill = Instantiate(dropPrefab);
            oneSpill.transform.position = pourPoint + Random.insideUnitSphere * dropPositionOffset;
            oneSpill.transform.localScale *= Random.Range(dropMinScaleMultiplier, 
                                                          dropMaxScaleMultiplier);
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            MeshRenderer renderer = oneSpill.GetComponent<MeshRenderer>();
            props.SetColor("_Color", color);
            renderer.SetPropertyBlock(props);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            Vector3 force = randomForce ? new Vector3(Random.value - 0.5f, 
                                        Random.value * 0.1f - 0.2f, 
                                        Random.value - 0.5f) : Vector3.zero;
            Vector3 pourForce = pourDirection.normalized * (modifier + 0.5f) * pourForceMultipilier;
            oneSpill.GetComponent<Rigidbody>().AddForce(force + pourForce);
            oneSpill.GetComponent<DropBehavior>().ingredient = ingredient;
        }
    }

    public void DripForSeconds(float seconds)
    {
        StartCoroutine("StartDrip", seconds);
    }

    public IEnumerator StartDrip(float seconds)
    {
        active = true;
        yield return new WaitForSeconds(seconds);
        active = false;
    }


}
