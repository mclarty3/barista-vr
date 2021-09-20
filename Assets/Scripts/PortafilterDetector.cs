﻿using System.Collections;
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

    private GameObject portafilter;
    private GameObject portafilterHighlight;
    private GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
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
            } else {
                Detach(other.gameObject);
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
        portafilter = obj.transform.parent.gameObject;
        portafilterHighlight.SetActive(false);
        portafilter.transform.GetComponent<Rigidbody>().isKinematic = true;
        groundsStartPos = portafilter.transform.Find("GroundsStartPos");
        groundsEndPos = portafilter.transform.Find("GroundsEndPos");

        if (portafilterAttached == PortafilterPos.EspressoMachine) {
            espressoMachineText.text = "Press button\n\nto drip espresso";
            espressoMachineAudioSource.clip = portafilterAttachSound;
            espressoMachineAudioSource.Play();
        }
    }

    void Detach(GameObject obj)
    {
        if (portafilterAttached == PortafilterPos.EspressoMachine) {
            espressoMachineText.text = "No portafilter";
            espressoMachineAudioSource.clip = portafilterDetachSound;
            espressoMachineAudioSource.Play();
        }
        portafilterAttached = PortafilterPos.None;
        portafilter = null;
        portafilterHighlight.SetActive(true);
        obj.transform.parent.GetComponent<Rigidbody>().isKinematic = false;
        groundsStartPos = null;
        groundsEndPos = null;
    }

    /* Filling portafilter with grounds from coffee grinder */

    public void TryFillPortafilter() 
    {
        if (portafilterAttached == PortafilterPos.Grinder && gm.espressoStatus == GameManager.EspressoStatus.None)
        {
            FillPortafilter();
        }
    }

    void FillPortafilter()
    {
        if (portafilter != null) {
            SetPortafilterHover(false);
            beanGrinderAudioSource.Play();
            gm.espressoStatus = GameManager.EspressoStatus.Fresh;
            GameObject grounds = portafilter.transform.Find("CoffeeGrounds").gameObject;
            grounds.SetActive(true);
            grounds.GetComponent<Renderer>().material = freshGroundsMat;
            grounds.transform.position = groundsStartPos.position;
            StartCoroutine("MoveGrounds", grounds);
        }
    }

    IEnumerator MoveGrounds(GameObject grounds)
    {
        Vector3 currentPos = groundsStartPos.position;
        float t = 0f;
        while (t < 1) {
            t += Time.deltaTime / 6;
            grounds.transform.position = Vector3.Lerp(currentPos, groundsEndPos.position, t);
            yield return null;
        }     
        SetPortafilterHover(true);
    }

    /* Dripping espresso from fresh filled portafilter */
    public void TryDripEspresso()
    {
        if (portafilterAttached == PortafilterPos.EspressoMachine && gm.espressoStatus == GameManager.EspressoStatus.Fresh) {
            DripEspresso();
        }
    }

    public void DripEspresso()
    {
        if (portafilter != null) {
            SetPortafilterHover(false);
            espressoMachineAudioSource.volume = 1;
            espressoMachineAudioSource.clip = espressoDripSound;
            espressoMachineAudioSource.Play();
            espressoMachineAudioSource.volume = 0.5f;
            gm.espressoStatus = GameManager.EspressoStatus.Used;
            portafilter.transform.Find("CoffeeGrounds").gameObject.GetComponent<Renderer>().material = usedGroundsMat;
            portafilter.transform.Find("LiquidSpout1").GetComponent<LiquidSpout>().active = true;
            portafilter.transform.Find("LiquidSpout2").GetComponent<LiquidSpout>().active = true;
            espressoMachineText.text = "Dripping\n\nespresso...";
            StartCoroutine("StartEspressoStream");
        }
    }

    IEnumerator StartEspressoStream()
    {
        float t = 0f;
        while (t < 1) {
            t += Time.deltaTime / 19;
            yield return null;
        }     
        SetPortafilterHover(true);
        portafilter.transform.Find("LiquidSpout1").GetComponent<LiquidSpout>().active = false;
        portafilter.transform.Find("LiquidSpout2").GetComponent<LiquidSpout>().active = false;
        espressoMachineText.text = "Remove\n\nportafilter";
    }

    void SetPortafilterHover(bool toggle)
    {
        if (toggle == true)
        {
            foreach (IgnoreHovering ignoreHovering in portafilter.GetComponentsInChildren<IgnoreHovering>())
            {
            Destroy(ignoreHovering);
            }
            
        }
        else {
            foreach (Collider collider in portafilter.GetComponentsInChildren<Collider>())
            {
                collider.gameObject.AddComponent<IgnoreHovering>();
            }
        }
    }
}
