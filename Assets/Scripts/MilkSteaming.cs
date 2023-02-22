using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnitySimpleLiquid;

public class MilkSteaming : MonoBehaviour
{
    [Tooltip("How quickly the steam wand will heat the milk, in Farenheit")]
    public float degreesPerSecond = 4f;
    public bool toggleSteamWand = false;
    private bool touchingMilk = false;
    private bool steamWandOn = false;
    private bool steamedMilk = false;
    // public ImprovedLiquid milk = null;
    public IngredientManager milk = null;
    public Transform steamWandBottom;
    public Transform steamWandTop;
    public AudioSource audioSource;
    public AudioClip touchingMilkStartSound;
    public AudioClip touchingMilkSoundLoop;
    public AudioClip noMilkSoundLoop;
    public AudioClip touchingMilkEndSound;
    public AudioClip noMilkEndSound;
    public ParticleSystem steamParticles;
    public Text milkStatusText;

    private Vector3 milkSurfaceColliderPos;

    System.Func<string> GetMilkTempText;

    // Start is called before the first frame update
    void Start()
    {
        GetMilkTempText = () => System.Math.Round(milk.Temperature, 1).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (toggleSteamWand)
        {
            SetSteamWandOn(!steamWandOn);
            toggleSteamWand = false;
        }

        if (steamWandOn && milk != null) {
            milk.Temperature += degreesPerSecond * Time.deltaTime;
            milkStatusText.text = "Steaming\n\nMilk temp\n\n" + GetMilkTempText() + "°F";

            if (milk.Temperature >= 125 && !steamedMilk)
            {
                milk.SetMilkSteamed();
                steamedMilk = true;
            }
        }
    }

    private bool DetectMilkPitcher(GameObject other)
    {
        return other.name == "Fluid" && other.transform.parent.name == "MilkPitcher";
    }

    void OnTriggerEnter(Collider other)
    {
        if (DetectMilkPitcher(other.gameObject)) {
            SetTouchingMilk(true, other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (DetectMilkPitcher(other.gameObject)) {
            SetTouchingMilk(false);
        }
    }

    public void SetSteamWandOn(bool active)
    {
        steamWandOn = active;

        if (steamWandOn) {
            if (!touchingMilk) {
                // Play empty steam wand sound
                audioSource.Stop();
                audioSource.clip = noMilkSoundLoop;
                audioSource.loop = true;
                audioSource.Play();
                milkStatusText.text = "Steaming...\n\nNo milk\n\n";
            } else {
                SteamMilk();
            }
            steamParticles.Play();
        } else {
            if (!touchingMilk) {
                // Play empty steam wand off sound
                audioSource.Stop();
                audioSource.clip = noMilkEndSound;
                audioSource.loop = false;
                audioSource.Play();
                milkStatusText.text = "No milk";
            } else {
                // Play milk steam wand off sound
                StopSteamMilk(false);
            }

            steamParticles.Stop();
        }
    }

    public void SetTouchingMilk(bool touching, GameObject liquidSurface = null)
    {
        if (touching && liquidSurface != null &&
            liquidSurface.transform.parent.TryGetComponent(out milk) &&
            milk.Temperature != -1)
        {
            touchingMilk = true;
            if (milk.Temperature >= 125) {
                steamedMilk = true;
            }
        } else {
            touchingMilk = false;
            milk = null;
            steamedMilk = false;
        }

        if (steamWandOn) {
            if (touchingMilk) {
                SteamMilk();
            } else {
                StopSteamMilk(true);
            }
        } else {
            if (touchingMilk) {
                milkStatusText.text = "Milk temp\n\n" + GetMilkTempText() + "°F";
            } else {
                milkStatusText.text = "No milk";
            }
        }
    }

    void SteamMilk()
    {
        milkStatusText.text = "Steaming\n\nMilk temp\n\n" + GetMilkTempText() + "°F";
        StartCoroutine("PlayMilkSteamWandSound");
    }

    void StopSteamMilk(bool steamWandStillOn)
    {
        if (steamWandStillOn) {
            audioSource.Stop();
            audioSource.clip = noMilkSoundLoop;
            audioSource.loop = true;
            audioSource.Play();
            milkStatusText.text = "Steaming...\n\nNo milk\n\n";
        } else {
            audioSource.Stop();
            audioSource.clip = touchingMilkEndSound;
            audioSource.loop = false;
            audioSource.Play();
            milkStatusText.text = "Milk temp\n\n" + GetMilkTempText() + "°F";
        }
    }

    IEnumerator PlayMilkSteamWandSound()
    {
        audioSource.Stop();
        audioSource.clip = touchingMilkStartSound;
        audioSource.loop = false;
        audioSource.Play();
        yield return new WaitForSeconds(touchingMilkStartSound.length);
        audioSource.clip = touchingMilkSoundLoop;
        audioSource.loop = true;
        audioSource.Play();
    }
}
