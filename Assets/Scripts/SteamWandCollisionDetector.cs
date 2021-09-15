using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamWandCollisionDetector : MonoBehaviour
{

    public MilkSteaming steamWand;
    public Transform steamWandBottom;
    public Transform steamWandTop;
    private bool touching = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y > steamWandBottom.position.y && transform.position.y < steamWandTop.position.y)
        {
            if (!touching) {
                steamWand.SetTouchingMilk(true);
            }
        }
        else 
        {
            if (touching) {
                steamWand.SetTouchingMilk(false);
            }
        }
    }


    /*void OnTriggerEnter(Collider other)
    {
        if (other.tag == "SteamWand")
        {
            steamWand.SetTouchingMilk(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "SteamWand")
        {
            steamWand.SetTouchingMilk(false);
        }
    }*/
}
