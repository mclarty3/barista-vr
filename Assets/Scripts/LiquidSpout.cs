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

            // Pour liquid
            if (angle > 90) {
                float modifier = (angle / 90) - 1;  // Clamp angle between 0 and 1
                drops = Mathf.FloorToInt(Mathf.Lerp(5, 20, modifier));
                for (int i = 0; i < drops; i++) {
                    GameObject oneSpill = Instantiate(dropPrefab);
                    oneSpill.transform.position = this.gameObject.transform.position + Random.insideUnitSphere * 0.05f;
                    oneSpill.transform.localScale *= Random.Range(0.45f, 0.65f);
                    oneSpill.GetComponent<Renderer>().material.color = dropColour;
                    Vector3 force = new Vector3(Random.value - 0.5f, Random.value * 0.1f - 0.2f, Random.value - 0.5f);
                    oneSpill.GetComponent<Rigidbody>().AddForce(force);
                }
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
