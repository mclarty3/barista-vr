using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public ParticleSystem particles;
    public int particleFlowRate = 2500;
    public bool isPouring = false;
    public float totalPoured = 0;

    [System.NonSerialized]
    public Dictionary<Ingredient, float> ingredients;
    [System.NonSerialized]
    public Color liquidColor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartPouring(Dictionary<Ingredient, float> ingredientAmounts)
    {
        ingredients = ingredientAmounts;
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
        totalPoured += drops;
        Debug.Log("Total poured: " + totalPoured);
    }

    public void StopPouring()
    {
        isPouring = false;
        var em = particles.emission;
        em.rateOverTime = 0;
    }

    // void OnParticleTrigger()
    // {
    //     List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
    //     List<ParticleSystem.Particle> exit = new List<ParticleSystem.Particle>();
    //     particles.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
    //     particles.GetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);
    //     for (int i = 0; i < enter.Count; i++)
    //     {
    //         ParticleSystem.Particle p = enter[i];
    //         p.remainingLifetime = 0;
    //         enter[i] = p;
    //     }
    //     for (int i = 0; i < exit.Count; i++)
    //     {
    //         ParticleSystem.Particle p = exit[i];
    //         p.remainingLifetime = 0;
    //         exit[i] = p;
    //     }
    //     particles.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
    //     particles.SetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);
    // }
}
