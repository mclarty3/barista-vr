using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Temp : MonoBehaviour
{
    // Start is called before the first frame update

    public ImprovedLiquid pitcher1;
    public ImprovedLiquid pitcher2;

    void Start()
    {
        this.GetComponent<Text>().text = "Pitcher 1: \nPitcher 2:";
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<Text>().text = "Pitcher 1: " + pitcher1.GetVolumeInOz(pitcher1.GetVolumeFilled()) + "\nPitcher 2: " + pitcher2.GetVolumeInOz(pitcher2.GetVolumeFilled());
    }
}
