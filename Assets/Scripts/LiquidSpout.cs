using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidSpout : MonoBehaviour
{
    public GameObject dropPrefab;
    public Color dropColour;
    public bool angleDependent = true;
    public bool active = false;
    public int drops = 10;
    public int dropsPerSecond = -1;

    [Header("Pour settings")]
    [Tooltip("The minimum number of drops that will be instantiated each FixedUpdate")]
    public int minDropsPerFrame = 1;
    [Tooltip("The maximum number of drops that will be instantiated each FixedUpdate")]
    public int maxDropsPerFrame = 25;
    [Tooltip("The multiplier for random drop position variations within a unit sphere surrounding the spout")]
    public float dropPositionOffset = 0.05f;
    [Tooltip("The minimum multiplier for random drop size variations")]
    public float dropMinScaleMultiplier = 0.25f;
    [Tooltip("The maximum multiplier for random drop size variations")]
    public float dropMaxScaleMultiplier = 1.25f;
    [Tooltip("Force multiplier to determine how heavily the drops are pushed towards the up vector of the spout")]
    public float pourForceMultipilier = 2;

    private Vector3 liquidOutVect;
    private float lastDropTime;

    // Start is called before the first frame update
    void Start()
    {
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
            if (angle > 90) {
                PourLiquid(angle: angle);
            }
        } else if (active && dropsPerSecond == -1) {
            for (int i = 0; i < drops; i++) {
                GameObject oneSpill = Instantiate(dropPrefab);
                oneSpill.transform.position = this.gameObject.transform.position + Random.insideUnitSphere * 0.01f;
                oneSpill.transform.localScale *= Random.Range(0.45f, 0.65f);
                oneSpill.GetComponent<Renderer>().material.color = dropColour;
                Vector3 force = new Vector3(Random.value - 0.5f, Random.value * 0.1f - 0.2f, Random.value - 0.5f);
                oneSpill.GetComponent<Rigidbody>().AddForce(force);
            }
        } else if (active && dropsPerSecond != -1) {
            if (Time.time - lastDropTime > 1.0f / dropsPerSecond) {
                GameObject oneSpill = Instantiate(dropPrefab);
                oneSpill.transform.position = this.gameObject.transform.position + Random.insideUnitSphere * 0.01f;
                oneSpill.transform.localScale *= Random.Range(0.45f, 0.65f);
                oneSpill.GetComponent<Renderer>().material.color = dropColour;
                Vector3 force = new Vector3(Random.value - 0.5f, Random.value * 0.1f - 0.2f, Random.value - 0.5f);
                oneSpill.GetComponent<Rigidbody>().AddForce(force);
                lastDropTime = Time.time;
            }
        }
    }

    void PourLiquid(int drops = -1, float modifier = -1, float angle = -1, float force_modifier = 1) {
        if (drops == -1) {
            if (modifier == -1) {
                modifier = angle == -1 ? Random.value : (angle / 90) - 1;
            }
            drops = Mathf.FloorToInt(Mathf.Lerp(minDropsPerFrame, maxDropsPerFrame, modifier));
        }

        for (int i = 0; i < drops; i++) {
            GameObject oneSpill = Instantiate(dropPrefab);
            oneSpill.transform.position = this.gameObject.transform.position + Random.insideUnitSphere * dropPositionOffset;
            oneSpill.transform.localScale *= Random.Range(dropMinScaleMultiplier, dropMaxScaleMultiplier);
            oneSpill.GetComponent<Renderer>().material.color = dropColour;
            Vector3 force = new Vector3((Random.value - 0.5f) * force_modifier, Random.value * 0.1f - 0.2f, (Random.value - 0.5f) * force_modifier);
            Vector3 pourForce = transform.up.normalized * pourForceMultipilier * (modifier + 0.5f);
            oneSpill.GetComponent<Rigidbody>().AddForce(force + pourForce);
        }
    }

    public static void PourLiquid(Vector3 pourPoint, Vector3 pourDirection, float minDropsPerFrame, float maxDropsPerFrame,
                           GameObject dropPrefab, float dropPositionOffset, float dropMinScaleMultiplier, float dropMaxScaleMultiplier,
                           Color dropColour, float pourForceMultipilier, int drops=-1, float modifier=-1, float angle=-1) {
        if (drops == -1) {
            if (modifier == -1) {
                modifier = angle == -1 ? Random.value : (angle / 90) - 1;
            }
            drops = Mathf.FloorToInt(Mathf.Lerp(minDropsPerFrame, maxDropsPerFrame, modifier));
        }

        for (int i = 0; i < drops; i++) {
            GameObject oneSpill = Instantiate(dropPrefab);
            oneSpill.transform.position = pourPoint + Random.insideUnitSphere * dropPositionOffset;
            oneSpill.transform.localScale *= Random.Range(dropMinScaleMultiplier, dropMaxScaleMultiplier);
            oneSpill.GetComponent<Renderer>().material.color = dropColour;
            Vector3 force = new Vector3(Random.value - 0.5f, Random.value * 0.1f - 0.2f, Random.value - 0.5f);
            Vector3 pourForce = pourDirection.normalized * (modifier + 0.5f) * pourForceMultipilier;
            oneSpill.GetComponent<Rigidbody>().AddForce(force + pourForce);
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
