using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiquidVolumeFX;

public class LiquidCatcher : MonoBehaviour
{
    private ImprovedLiquid improvedLiquid;

    // Start is called before the first frame update
    void Start()
    {
        if (!gameObject.transform.parent.TryGetComponent<ImprovedLiquid>(out improvedLiquid)) {
            Debug.LogWarning(name + " has LiquidCatcher script but no ImprovedLiquid script");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCatchLiquid(DropBehavior drop) {
        improvedLiquid.AddDrop(drop);
    }
}
