using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamWandCollisionDetector : MonoBehaviour
{

    public MilkSteaming steamWand;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        if (other.gameObject.name == "Fluid") {
            steamWand.SetTouchingMilk(true, other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Fluid") {
            steamWand.SetTouchingMilk(false);
        }
    }
}
