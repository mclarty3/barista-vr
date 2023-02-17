using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiquidVolumeFX;

public class LiquidCatcher : MonoBehaviour
{
    private ImprovedLiquid improvedLiquid;

    // Start is called before the first frame update
    void Start()
    {
        if (!gameObject.transform.parent.TryGetComponent<ImprovedLiquid>(out improvedLiquid)) {
            Debug.LogWarning(name + " has LiquidCatcher script but no ImprovedLiquid script");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // void OnParticleCollision(GameObject other) {
    //     ParticleManager particleManager = other.GetComponent<ParticleManager>();
    //     ParticleSystem ps = other.GetComponent<ParticleSystem>();
    //     List<ParticleCollisionEvent> events = new List<ParticleCollisionEvent>();
    //     int numCollisions = ParticlePhysicsExtensions.GetCollisionEvents(ps, gameObject, events);
    //     foreach (KeyValuePair<Ingredient, float> ingredient in particleManager.ingredients) {
    //         for (int i = 0; i < numCollisions * ingredient.Value; i++) {
    //             OnCatchLiquid(ingredient.Key);
    //         }
    //     }
    // }

    public void OnCatchLiquid(Ingredient ingredient) {
        if (improvedLiquid != null) {
            improvedLiquid.AddDrop(ingredient);
        }
    }

    public void OnCatchLiquid(DropBehavior drop) {
        improvedLiquid.AddDrop(drop);
    }
}
