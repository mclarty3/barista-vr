using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CupStack : MonoBehaviour
{

    public GameObject dormantCup;

    private GameObject grabbedCup;
    private GameObject newCup;
    public GameObject cupPrefab;
    public Transform cupSpawnPoint;

    private bool grabbed = false;
    private float leaveProjectedDistance = 0.065f;
    private float leaveHeightDifference = 0.19f;

    // Start is called before the first frame update
    void Start()
    {
        newCup = dormantCup;
    }

    // Update is called once per frame
    void Update()
    {
        // Cup grabbed by hand
        if (!grabbed && !dormantCup.GetComponent<Rigidbody>().useGravity)
        {
            OnCupGrabbed();
            grabbed = true;
        }
        else if (grabbed) 
        {
            Vector3 grabbedCenter = grabbedCup.transform.Find("Pipe").GetComponent<Renderer>().bounds.center;
            if (Vector2.Distance(Vector3.ProjectOnPlane(this.transform.position, Vector3.up), Vector3.ProjectOnPlane(grabbedCenter, Vector3.up)) >= leaveProjectedDistance ||
                grabbedCenter.y - this.transform.position.y >= leaveHeightDifference) 
            {
                OnCupExit();
                grabbed = false;
            }
        }
    }

    private void OnCupGrabbed() 
    {
        ToggleCupColliders(dormantCup, false);
        dormantCup.GetComponent<Rigidbody>().isKinematic = false;
        grabbedCup = newCup;
        newCup = Instantiate(cupPrefab, cupSpawnPoint.position, Quaternion.LookRotation(Vector3.back, Vector3.down)) as GameObject;
        newCup.GetComponent<Rigidbody>().isKinematic = true;
    }

    void OnCupExit()
    {
        ToggleCupColliders(grabbedCup, true);
        dormantCup = newCup;
    }

    void ToggleCupColliders(GameObject cup, bool active)
    {
        Collider[] cupColliders = cup.GetComponentsInChildren<Collider>();
        
        foreach (Collider collider in cupColliders) {
            collider.enabled = active;
        }
    }
}
