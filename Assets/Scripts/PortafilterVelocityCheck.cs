using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortafilterVelocityCheck : MonoBehaviour
{

    public Text text;
    private Vector3 lastPos;
    private Vector3 currentPos;
    private Vector3 lastVelocity;
    private Vector3 currentVelocity;
    private Vector3 acceleration;
    private Vector3 projectedAcceleration;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("GetAcceleration");
        StartCoroutine("SetText");
    }

    // Update is called once per frame
    void Update()
    {
        projectedAcceleration = Vector3.Project(acceleration, transform.up);
        
    }

    IEnumerator GetAcceleration()
    {
        lastPos = transform.position;
        yield return new WaitForFixedUpdate();
        currentPos = transform.position;
        lastVelocity = currentPos - lastPos;
        yield return new WaitForFixedUpdate();
        lastPos = transform.position;
        yield return new WaitForFixedUpdate();
        currentPos = transform.position;
        currentVelocity = currentPos - lastPos;
        acceleration = currentVelocity - lastVelocity;
        StartCoroutine("GetAcceleration");
    }

    IEnumerator SetText()
    {
        //Debug.Log(projectedAcceleration.magnitude);
        if (projectedAcceleration.magnitude >= 0.01f)
        {
            text.text = "Hit!";
            yield return new WaitForSeconds(3f);
        }
        else {
            text.text = "";
        }
        StartCoroutine("SetText");
    }
}
