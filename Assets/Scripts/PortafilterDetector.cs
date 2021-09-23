using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class PortafilterDetector : MonoBehaviour
{
    public enum PortafilterPos
    {
        None,
        Grinder,
        EspressoMachine
    }

    public PortafilterPos portafilterAttached = PortafilterPos.None;
    public Transform portafilterPos;
    public Material freshGroundsMat;
    public Material usedGroundsMat;
    public AudioSource beanGrinderAudioSource;
    public AudioSource espressoMachineAudioSource;
    public AudioClip portafilterAttachSound;
    public AudioClip portafilterDetachSound;
    public AudioClip espressoDripSound;
    public Transform groundsStartPos;
    public Transform groundsEndPos;
    [SerializeField]
    public PortafilterPos detectorType;
    public Text espressoMachineText;

    // private GameObject portafilter;
    private Portafilter portafilter;
    private GameObject portafilterHighlight;
    private GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        portafilter = null;
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        portafilterHighlight = this.transform.GetChild(0).gameObject;
        portafilterHighlight.SetActive(false);
        portafilterAttachSound = Resources.Load<AudioClip>("Audio/PortafilterAttach");
        portafilterDetachSound = Resources.Load<AudioClip>("Audio/PortafilterDetach");
        espressoDripSound = Resources.Load<AudioClip>("Audio/EspressoMachine");
    }

    // Update is called once per frame
    void Update()
    {
        if (portafilterAttached != PortafilterPos.None) {
            portafilter.transform.position = portafilterPos.position;
            portafilter.transform.rotation = portafilterPos.rotation;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.transform.parent.tag == "Portafilter") {
            if (other.transform.parent.GetComponent<Rigidbody>().useGravity) {
                if (portafilterAttached == PortafilterPos.None) {
                    Debug.Log("Attaching because portafilterAttached = " + portafilterAttached);
                    Attach(other.gameObject);
                }
            } else if (portafilter != null) {
                Detach(other.gameObject);
            } else if (!portafilterHighlight.activeSelf) {
                portafilterHighlight.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        portafilterHighlight.SetActive(false);
    }

    void Attach(GameObject obj)
    {
        portafilterAttached = detectorType;
        portafilter = obj.transform.parent.gameObject.GetComponent<Portafilter>();
        portafilter.detector = this;
        portafilterHighlight.SetActive(false);

        if (detectorType == PortafilterPos.EspressoMachine) {
            espressoMachineText.text = "Press button\n\nto drip espresso";
            espressoMachineAudioSource.clip = portafilterAttachSound;
            espressoMachineAudioSource.Play();
        }
    }

    void Detach(GameObject obj)
    {
        if (detectorType == PortafilterPos.EspressoMachine) {
            espressoMachineText.text = "No portafilter";
            espressoMachineAudioSource.clip = portafilterDetachSound;
            espressoMachineAudioSource.Play();
        }
        portafilter.Detach();
        portafilterAttached = PortafilterPos.None;
        portafilter = null;
        portafilterHighlight.SetActive(true);
    }

    /* Filling portafilter with grounds from coffee grinder */

    public void TryFillPortafilter() 
    {
        if (portafilter != null && 
            detectorType == PortafilterPos.Grinder && 
            portafilter.espressoStatus == Portafilter.EspressoStatus.None) 
        {
            portafilter.Fill();
            beanGrinderAudioSource.Play();
        }
    }

    public void TryDripEspresso()
    {
        if (portafilter != null && portafilter.detector.detectorType == PortafilterPos.EspressoMachine)
        {
            espressoMachineAudioSource.volume = 1;
            espressoMachineAudioSource.clip = espressoDripSound;
            espressoMachineAudioSource.Play();
            espressoMachineAudioSource.volume = 0.5f;
            espressoMachineText.text = "Dripping\n\nespresso...";
            portafilter.DripEspresso();
        }
    }

    public void StopDripEspresso()
    {
        espressoMachineAudioSource.volume = 0.5f;
        espressoMachineText.text = "Please remove portafilter";
    }
}