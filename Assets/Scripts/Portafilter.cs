using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using UnitySimpleLiquid;

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

    // public LiquidSpout lvSpout1;
    // public LiquidSpout lvSpout2;

    public SplitController lvSpout1;
    public SplitController lvSpout2;

    private GameObject grounds;
    private bool recentlyRemoved = false;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        grounds = transform.Find("CoffeeGrounds").gameObject;
        espressoStatus = EspressoStatus.None;
        detector = null;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (recentlyRemoved)
        {
            if (rb.useGravity)
            {
                recentlyRemoved = false;
                ToggleColliders(true);
                Attach(detector);
            }

            var distance = Vector3.Distance(transform.position, detector.transform.position);
            if (distance > 0.1f)
            {
                recentlyRemoved = false;
                ToggleColliders(true);
                detector = null;
            }
        }
    }

    public void Attach(PortafilterDetector detector)
    {
        this.detector = detector;
        rb.isKinematic = true;
    }

    public void Detach()
    {
        rb.isKinematic = false;
        recentlyRemoved = true;
        ToggleColliders(false);
    }

    public void Fill()
    {
        ToggleHover(false);
        espressoStatus = EspressoStatus.Fresh;
        grounds.transform.position = groundsStartPos.position;
        grounds.SetActive(true);
        grounds.GetComponent<Renderer>().material = freshGroundsMat;
        StartCoroutine(MoveGrounds());
    }

    IEnumerator MoveGrounds()
    {
        Vector3 currentPos = groundsStartPos.position;
        float t = 0f;
        while (t < 1) {
            t += Time.deltaTime / 6 * groundsSpeed;
            grounds.transform.position = Vector3.Lerp(currentPos, groundsEndPos.position, t);
            yield return null;
        }
        ToggleHover(true);
    }
    public void DripEspresso()
    {
        ToggleHover(false);
        grounds.GetComponent<Renderer>().material = usedGroundsMat;
        // lvSpout1.active = true;
        // lvSpout2.active = true;
        lvSpout1.manualPour = true;
        lvSpout2.manualPour = true;
        espressoStatus = EspressoStatus.Used;
        StartCoroutine(StartEspressoStream());
    }

    IEnumerator StartEspressoStream()
    {
        float t = 0f;
        while (t < 1) {
            t += Time.deltaTime / 19;
            yield return null;
        }
        ToggleHover(true);
        // lvSpout1.active = false;
        // lvSpout2.active = false;
        lvSpout1.manualPour = false;
        lvSpout2.manualPour = false;
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

    void ToggleColliders(bool toggle)
    {
        foreach (Collider collider in transform.GetComponentsInChildren<Collider>()) {
            collider.enabled = toggle;
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
