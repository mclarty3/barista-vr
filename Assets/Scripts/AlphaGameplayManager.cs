using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlphaGameplayManager : MonoBehaviour
{

    public Text orderText;
    public Text timeText;
    public Text performanceText;
    public bool start = false;
    public bool stop = false;

    private float startTime;
    private Beverage beverage;
    private GameObject beverageObject;

    // Start is called before the first frame update
    void Start()
    {
        StopRound();
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            StartRound();
            start = false;
        } else if (stop) {
            StopRound();
            stop = false;
        } else if (startTime != -1)
        {
            float timeElapsed = Time.time - startTime;
            timeText.text = "Time elapsed: " + timeElapsed.ToString("00:00.000");
        }
    }

    public void StartRound()
    {
        if (beverageObject != null)
        {
            Destroy(beverageObject);
            beverageObject = null;
        }
        beverage = Beverage.GenerateBeverage();
        orderText.text = "Order: " + beverage.name;
        startTime = Time.time;
        performanceText.text = "Place the crafted drink in the circle\nto see your performance";
    }

    public void StopRound()
    {
        if (beverageObject != null)
        {
            Destroy(beverageObject);
            beverageObject = null;
        }
        orderText.text = "Order: No order yet";
        timeText.text = "Time elapsed: 00:00.000";
        performanceText.text = "Press start to receive\nan order and begin";
        startTime = -1;
    }

    public void CompleteRound(ImprovedLiquid liquid)
    {
        beverageObject = liquid.gameObject;
        beverage.DebugBeverageScore(liquid, out string message);
        performanceText.text = message;
        startTime = -1;
    }
}
