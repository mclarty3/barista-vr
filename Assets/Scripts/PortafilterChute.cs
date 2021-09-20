using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortafilterChute : MonoBehaviour
{
    GameManager gm;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.tag == "Portafilter") {
            float velocity = other.transform.parent.GetComponent<Rigidbody>().velocity.magnitude;
            // TODO: Replace with parameterized value (probably in Portafilter?)
            if (velocity >= 3) {
                gm.espressoStatus = GameManager.EspressoStatus.None;
                other.transform.parent.Find("CoffeeGrounds").gameObject.SetActive(false);
            }
        }   
    }
}
