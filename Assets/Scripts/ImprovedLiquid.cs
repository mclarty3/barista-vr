using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiquidVolumeFX;

public class ImprovedLiquid : MonoBehaviour
{
    class LiquidAmount
    {
        public Color color;
        public float amount;

        public LiquidAmount() { }

        public LiquidAmount(Color color, float amount) 
        {
            this.color = color;
            this.amount = amount;
        }
    }

    public GameObject liquidDropPrefab;
    public int numDropsPerSecond = 10;
    public GameObject liquidSurfaceCollider;
    
    public LiquidVolume lv;

    private Vector3 spillPoint;
    public float meshVolume;
    private Dictionary<Color, float> colorRatio = new Dictionary<Color, float>();
    private List<LiquidAmount> colorAmounts = new List<LiquidAmount>();
    private GameManager gameManager;
    private float referenceVolume;
    private int numOzInReferenceVolume = 20;
    public int dropsPerOz = 100;

    // Start is called before the first frame update
    void Start()
    {
        if (!TryGetComponent<LiquidVolume>(out lv)) {
            Debug.LogError(name + " has ImprovedLiquid script but no LiquidVolume script.");
        }

        if (lv.level > 0) {
            colorAmounts.Add(new LiquidAmount(lv.liquidColor1, lv.level));
        }

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        referenceVolume = GetMeshVolume.VolumeOfMesh(
            gameManager.referenceLiquidVolume.gameObject.GetComponent<MeshFilter>().sharedMesh, gameManager.referenceLiquidVolume.gameObject.transform);
        meshVolume = GetMeshVolume.VolumeOfMesh(lv.gameObject.GetComponent<MeshFilter>().sharedMesh, lv.gameObject.transform);
        Debug.Log(transform.parent.name + " has " + meshVolume + " volume and " + GetVolumeInOz(meshVolume) + " oz with " + GetVolumeInOz(GetVolumeFilled()) + " oz filled");
    }

    // Update is called once per frame
    void Update()
    {
        if (liquidSurfaceCollider != null)
        {    
            //Vector3 liquidPos = transform.parent.Find("Cylinder").transform.position;
            Vector3 liquidPos = transform.position;
            liquidPos.y = lv.liquidSurfaceYPosition;
            liquidSurfaceCollider.transform.position = liquidPos;
        }
    }

    // TODO: Improve this, use similar code to LiquidSpout for more realistic pouring
    void FixedUpdate()
    {
        Vector3 spillPos;
        float spillAmount;
        if (lv.GetSpillPoint(out spillPos, out spillAmount)) {
            int drops = Mathf.FloorToInt(Mathf.Lerp(5, 15, spillAmount / 0.5f));
            for (int i = 0; i < drops; i++) {
                SpillDrop(spillPos);
            }
            lv.level -= GetLevelFromDrops(drops);
            Debug.Log(drops + " drops lowers level by " + GetLevelFromDrops(drops));

            List<LiquidAmount> temp = new List<LiquidAmount>();
            foreach (LiquidAmount colorAmount in colorAmounts) {
                colorAmount.amount -= GetLevelFromDrops(drops);
                if (colorAmount.amount >= 0.05f)
                {
                    temp.Add(colorAmount);
                }
            }
        }
    }

    private void SpillDrop(Vector3 spillPos)
    {
        Debug.Log("Spilling one drop");
        GameObject oneDrop = Instantiate(liquidDropPrefab);
        oneDrop.transform.position = spillPos + Random.insideUnitSphere * 0.005f;
        oneDrop.transform.localScale *= Random.Range(0.45f, 0.65f);
        oneDrop.GetComponent<Renderer>().material.color = lv.liquidColor1;
        Vector3 force = new Vector3(Random.value - 0.5f, Random.value * 0.1f - 0.2f, Random.value - 0.5f);
        oneDrop.GetComponent<Rigidbody>().AddForce(force);

    }

    public void AddDrop(DropBehavior drop) {
        lv.level += GetLevelFromDrops();
        Debug.Log(1 + " drop raises level by " + GetLevelFromDrops());

        // Search colorAmounts for drop's colour
        bool hasColour = false;
        foreach (LiquidAmount colorAmount in colorAmounts) {
            if (colorAmount.color == drop.dropColor) {
                hasColour = true;
                colorAmount.amount += GetLevelFromDrops();
            }
        }
        if (!hasColour) {
            colorAmounts.Add(new LiquidAmount(drop.dropColor, GetLevelFromDrops()));
        } 
        
        foreach (LiquidAmount colorAmount in colorAmounts) {
            lv.liquidColor1 = Color.Lerp(lv.liquidColor1, colorAmount.color, colorAmount.amount);
        }

        /*if (lv.level < 0.1) 
            lv.liquidColor1 = drop.dropColor;
        else
            lv.liquidColor1 = Color.Lerp(lv.liquidColor1, drop.dropColor, (1 - lv.level) / 100);*/
    }

    public float GetVolumeFilled()
    {
        return meshVolume * lv.level;
    }

    public float GetVolumeInOz(float volume)
    {
        return 20f * volume / referenceVolume;
    }

    public float GetVolumeFromDrops(int numDrops)
    {
        return (referenceVolume * numDrops) / (20 * dropsPerOz);
    }

    public float GetLevelFromDrops(int numDrops = 1)
    {
        return GetVolumeFromDrops(numDrops) / meshVolume;
    }
}
