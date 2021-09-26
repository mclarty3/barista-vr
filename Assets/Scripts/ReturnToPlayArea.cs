using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToPlayArea : MonoBehaviour
{
    [Tooltip("The center of the play area; used for calculating distance")]
    public Transform origin;
    [Tooltip("The minimum distance the object must be from the origin before being respawned")]
    public float distance = 7f;
    [Tooltip("The time in seconds the object spends outside the zone before respawning")]
    public float timeBeforeRespawn = 3f;
    [Tooltip("The position and rotation the object will be respawned at")]
    public Transform respawnPoint;
    private Vector3 respawnPos;
    private Quaternion respawnRot;

    // Start is called before the first frame update
    void Start()
    {
        respawnPos = respawnPoint.position;
        respawnRot = respawnPoint.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, origin.position) > distance)
        {
            StartCoroutine(RespawnTimer());
        }
    }

    IEnumerator RespawnTimer()
    {
        float time = 0f;
        while (time < timeBeforeRespawn)
        {
            if (Vector3.Distance(transform.position, origin.position) <= distance) {
                yield break;
            }
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = respawnPos;
        transform.rotation = respawnRot;
    }
}
