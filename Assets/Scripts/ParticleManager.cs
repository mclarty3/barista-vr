using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public ParticleSystem particles;
    public bool isPouring { 
        get { return _isPouring;} 
        set { 
            Debug.Log("setting...");
            _isPouring = value;
            var em = particles.emission;
            em.rateOverTime = value ? 2500 : 0;
            Debug.Log(em.rateOverTime);
        }
    }
    [SerializeField]
    public bool _isPouring = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.position = transform.position - particles.transform.position;
        shape.rotation = transform.rotation.eulerAngles - particles.transform.rotation.eulerAngles;
    }
}
