using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Portafilter : MonoBehaviour
{
    public enum EspressoStatus
    {
        None,
        Fresh,
        Used
    }

    public Transform groundsStartPos;
    public Transform groundsEndPos;
    public float groundsSpeed = 1;
    public Material freshGroundsMat;
    public Material usedGroundsMat;

    public EspressoStatus espressoStatus;
    [System.NonSerialized]
    public PortafilterDetector detector;

    public LiquidSpout lvSpout1;
    public LiquidSpout lvSpout2;
    
    private GameObject grounds;

    // Start is called before the first frame update
    void Start()
    {
        grounds = transform.Find("CoffeeGrounds").gameObject;
        espressoStatus = EspressoStatus.None;
        detector = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attach(PortafilterDetector detector) 
    {
        this.detector = detector;
        transform.GetComponent<Rigidbody>().isKinematic = true;
    }

    public void Detach()
    {
        detector = null;
        transform.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void Fill()
    {
        ToggleHover(false);
        espressoStatus = EspressoStatus.Fresh;
        grounds.transform.position = groundsStartPos.position;
        grounds.SetActive(true);
        grounds.GetComponent<Renderer>().material = freshGroundsMat;
        StartCoroutine("MoveGrounds");
    }

    IEnumerator MoveGrounds()
    {
        Vector3 currentPos = groundsStartPos.position;
        float t = 0f;
        while (t < 1) {
            t += (Time.deltaTime / 6) * groundsSpeed;
            grounds.transform.position = Vector3.Lerp(currentPos, groundsEndPos.position, t);
            yield return null;
        }     
        ToggleHover(true);
    }
    public void DripEspresso()
    {
        ToggleHover(false);
        grounds.GetComponent<Renderer>().material = usedGroundsMat;
        lvSpout1.active = true;
        lvSpout2.active = true;
        espressoStatus = EspressoStatus.Used;
        StartCoroutine("StartEspressoStream");
    }

    IEnumerator StartEspressoStream()
    {
        float t = 0f;
        while (t < 1) {
            t += Time.deltaTime / 19;
            yield return null;
        }     
        ToggleHover(true);
        lvSpout1.active = false;
        lvSpout2.active = false;
        detector.StopDripEspresso();
    }

    void ToggleHover(bool toggle)
    {
        if (!toggle)
        {
            foreach (Collider collider in transform.GetComponentsInChildren<Collider>()) {
                collider.gameObject.AddComponent<IgnoreHovering>();
            }
        }
        else {
            foreach (var ignoreHovering in transform.GetComponentsInChildren<IgnoreHovering>()) {
                Destroy(ignoreHovering);
            }
        }
    }

    public void TrashEspresso()
    {
        if (espressoStatus != EspressoStatus.None) {
            grounds.SetActive(false);
            espressoStatus = EspressoStatus.None;
        }
    }
}
