using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrinkDetector : MonoBehaviour
{

    public AlphaGameplayManager agm;

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
        GameObject otherObj = FindParentWithTag(other.gameObject, "Cup");
        if (otherObj != null) {
            ImprovedLiquid liquid = otherObj.GetComponentInChildren<ImprovedLiquid>();
            if (liquid != null) {
                agm.CompleteRound(liquid);
            }
        }
    }

    public static GameObject FindParentWithTag(GameObject childObject, string tag)
    {
        Transform t = childObject.transform;
        while (t.parent != null)
        {
          if (t.parent.tag == tag)
          {
            return t.parent.gameObject;
          }
          t = t.parent.transform;
        }
        return null; // Could not find a parent with given tag.
    }
}
