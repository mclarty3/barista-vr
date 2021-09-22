using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MilkSteaming : MonoBehaviour
{
    public bool touchingMilk = false;
    public bool steamWandOn = false;
    public ImprovedLiquid milk;
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


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (milk.lv.liquidSurfaceYPosition > steamWandBottom.position.y && 
            milk.lv.liquidSurfaceYPosition < steamWandTop.position.y && 
            Mathf.Abs(steamWandBottom.position.x - milk.lv.transform.position.x) <= 0.04f && 
            Mathf.Abs(steamWandBottom.position.z - milk.lv.transform.position.z) <= 0.04f)
        {
            if (!touchingMilk) {
                SetTouchingMilk(true);
            }
        }
        else 
        {
            if (touchingMilk) {
                SetTouchingMilk(false);
            }
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
                // Play milk steam wand sound
                milkStatusText.text = "Steaming\n\nMilk temp\n\n145°F";
                StartCoroutine("PlayMilkSteamWandSound");
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
                audioSource.Stop();
                audioSource.clip = touchingMilkEndSound;
                audioSource.loop = false;
                audioSource.Play();
                milkStatusText.text = "Milk temp\n\n145°F";
            }
            
            steamParticles.Stop();
        }
    }

    public void SetTouchingMilk(bool touching)
    {
        touchingMilk = touching;

        

        if (steamWandOn) {
            if (touchingMilk) {
                // Play milk steam wand sound
                milkStatusText.text = "Steaming...\n\nMilk temp\n\n145°F";
                StartCoroutine("PlayMilkSteamWandSound");
            } else {
                // Play empty steam wand sound
                audioSource.Stop();
                audioSource.clip = noMilkSoundLoop;
                audioSource.loop = true;
                audioSource.Play();
                milkStatusText.text = "Steaming...\n\nNo milk";
            }
        }
        else {
            if (touching) {
                milkStatusText.text = "Milk temp\n\n145°F";
            } else {
                milkStatusText.text = "No milk";
            }
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
