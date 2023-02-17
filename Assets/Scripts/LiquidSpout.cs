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

    [Header("Pour Amount")]
    [Tooltip("The minimum number of drops that will be instantiated each FixedUpdate")]
    public int minDropsPerFrame = 1;
    [Tooltip("The maximum number of drops that will be instantiated each FixedUpdate")]
    public int maxDropsPerFrame = 25;
    [Tooltip("The minimum multiplier for random drop size variations")]
    public float dropMinScaleMultiplier = 0.25f;
    [Tooltip("The maximum multiplier for random drop size variations")]
    public float dropMaxScaleMultiplier = 1.25f;
    [Tooltip("Determines how heavily the drops are pushed towards the up vector of the spout")]
    public float pourForceMultipilier = 2;

    [Header("Pour Position")]
    [Tooltip("Determines random drop position variations within a unit sphere surrounding the spout")]
    public float dropRandomPositionMultiplier = 0.05f;
    [Tooltip("Determines how far forward the drops are offset from the spout's position")]
    public float dropForwardPositionOffset = 0.0f;
    [Tooltip("Determines how far up the drops are offset from the spout's position")]
    public float dropUpPositionOffset = 0.0f;
    [Tooltip("Determines how randomized the drop's position in a sphere around the pour point")]
    public float randomDropForceMultiplier = 0.1f;

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
        liquidOutVect = gameObject.transform.up;
        lastDropTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        liquidOutVect = gameObject.transform.up;
    }

    void FixedUpdate()
    {
        if (angleDependent) {
            float angle = Vector3.Angle(Vector3.up, liquidOutVect);
            if (angle > angleThreshold) {
                PourLiquid(angle: angle);
            }
        } else if (active) {
            if (dropsPerSecond == -1) {
                PourLiquid(drops, modifier: 1);
            } else {
                if (Time.time - lastDropTime > (1.0f / dropsPerSecond)) {
                    PourLiquid(drops: 1, modifier: 1);
                    lastDropTime = Time.time;
                }
            }
        }
    }

    void PourLiquid(int drops = -1, float modifier = -1, float angle = -1) {
        List<Ingredient> ingredients = new List<Ingredient>() { ingredient };
        PourLiquid(transform.position, transform.up.normalized, ingredients, dropColour, liquidTemperature, dropPrefab,
                   new Vector2(minDropsPerFrame, maxDropsPerFrame),
                   new Vector2(dropForwardPositionOffset, dropUpPositionOffset),
                   dropRandomPositionMultiplier,
                   new Vector2(dropMinScaleMultiplier, dropMaxScaleMultiplier),
                   pourForceMultipilier, drops, modifier, angle, angleThreshold);
    }

    public static void PourLiquid(Vector3 pourPoint, Vector3 pourDirection, List<Ingredient> ingredients,
                                  Color color, float temperature, GameObject dropPrefab,
                                  Vector2 minMaxDropsPerFrame,
                                  Vector2 dropPositionOffset,
                                  float dropRandomPositionMultiplier,
                                  Vector2 minxMaxDropScaleMultiplier,
                                  float pourForceMultipilier, int drops=-1, float modifier=-1, float angle=-1,
                                  float angleThreshold=90) {
        if (modifier == -1) {
            modifier = angle == -1 ? Random.value : (angle - angleThreshold) / (180 - angleThreshold);
        }
        if (drops == -1) {
            drops = Mathf.FloorToInt(Mathf.Lerp(minMaxDropsPerFrame[0], minMaxDropsPerFrame[1], modifier));
        }

        for (int i = 0; i < drops; i++) {
            // Designed to iterate equally over all ingredients
            Ingredient ingredient = new Ingredient(Ingredient.IngredientType.Undefined);
            try {
                ingredient = ingredients[i % ingredients.Count];
            } catch (System.DivideByZeroException) {
                Debug.LogWarning("No ingredients in liquid. Did you artifically raise the liquid level?");
            }
            ingredient.temperature = temperature;
            GameObject oneSpill = Instantiate(dropPrefab);
            oneSpill.transform.position = pourPoint + pourDirection * dropPositionOffset[0] +
                                          Vector3.up * dropPositionOffset[1] +
                                          Random.insideUnitSphere * dropRandomPositionMultiplier;
            oneSpill.transform.localScale *= Random.Range(minxMaxDropScaleMultiplier[0], minxMaxDropScaleMultiplier[1]);

            Vector3 pourForce = pourDirection.normalized * pourForceMultipilier;
            oneSpill.GetComponent<Rigidbody>().AddForce(pourForce);
            oneSpill.GetComponent<DropBehavior>().ingredient = ingredient;

            MaterialPropertyBlock props = new MaterialPropertyBlock();
            MeshRenderer renderer = oneSpill.GetComponent<MeshRenderer>();
            props.SetColor("_Color", color);
            renderer.SetPropertyBlock(props);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
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
