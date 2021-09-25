using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortafilterChute : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.tag == "Portafilter") {
            float velocity = other.transform.parent.GetComponent<Rigidbody>().velocity.magnitude;
            float angle = Vector3.Angle(other.transform.parent.transform.up, Vector3.up);
            if (velocity >= 4.0f && angle > 120) {
                Portafilter portafilter = other.transform.parent.GetComponent<Portafilter>();
                portafilter.TrashEspresso();
            }
        }   
    }
}
