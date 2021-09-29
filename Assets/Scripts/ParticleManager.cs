using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public ParticleSystem particles;
    public int particleFlowRate = 2500;
    public bool isPouring = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // ParticleSystem.ShapeModule shape = particles.shape;
        // // Debug.Log(transform.position.ToString() + " " + particles.transform.position.ToString() + " " + (transform.position - particles.transform.position).ToString());
        // shape.position = transform.position - particles.transform.position;
        // shape.rotation = (Quaternion.Inverse(particles.transform.rotation) * transform.rotation).eulerAngles;
    }

    public void StartPouring()
    {
        float start = Time.time;
        StartCoroutine(Pour(start));
    }

    IEnumerator Pour(float start)
    {
        yield return new WaitForSeconds(0.001f);
        isPouring = true;
        var em = particles.emission;
        em.rateOverTime = particleFlowRate;
        int drops = (int)(particleFlowRate * (Time.time - start));
        transform.parent.GetComponentInChildren<ImprovedLiquid>().ReduceLiquid(drops);
    }

    public void StopPouring()
    {
        isPouring = false;
        var em = particles.emission;
        em.rateOverTime = 0;
    }
}
