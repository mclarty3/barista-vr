using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortafilterChute : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.tag == "Portafilter") {
            float velocity = other.transform.parent.GetComponent<Rigidbody>().velocity.magnitude;
            if (velocity >= 4.0f) {
                Portafilter portafilter = other.transform.parent.GetComponent<Portafilter>();
                portafilter.TrashEspresso();
            }
        }   
    }
}
