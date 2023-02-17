using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortafilterChute : MonoBehaviour
{
    public float minimumVelocity = 1.0f;
    public float minimumAngle = 120f;
    public AudioSource trashAudio;

    void OnTriggerEnter(Collider other)
    {
        if (!other.transform.parent || !other.transform.parent.parent) {
            return;
        }
        var portafilter = other.transform.parent.parent.GetComponent<Portafilter>();

        if (portafilter == null || portafilter.tag != "Portafilter")
            return;

        float velocity = portafilter.GetComponent<Rigidbody>().velocity.magnitude;
        float angle = Vector3.Angle(portafilter.transform.up, Vector3.up);
        if (velocity >= minimumVelocity && angle > minimumAngle) {
            portafilter.TrashEspresso();
            trashAudio.Play();
        }
    }
}
