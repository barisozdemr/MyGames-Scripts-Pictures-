using System.Collections;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    public AudioSource audioSource;
    
    public AudioClip buttonClickedSoundClip;
    
    public AudioClip blockClickedSoundClip;
    public AudioClip rocketCreatedSoundClip;
    public AudioClip blockFellDownSoundClip;
    public AudioClip rocketShootedSoundClip;
    public AudioClip rocketHitSoundClip;

    public AudioClip boxDestroyedSoundClip;
    public AudioClip stoneDestroyedSoundClip;
    public AudioClip vaseCrackedSoundClip;
    public AudioClip vaseDestroyedSoundClip;
    
    public AudioClip gameWonSoundClip;
    public AudioClip gameOverSoundClip;

    private bool blockFellDownSoundAvailable = true;
    
    void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
    }
    
    public void playButtonClickedSound()
    {
        audioSource.PlayOneShot(buttonClickedSoundClip);
    }
    
    public void playBlockClickedSound()
    {
        audioSource.PlayOneShot(blockClickedSoundClip);
    }
    public void playRocketCreatedSound()
    {
        audioSource.PlayOneShot(rocketCreatedSoundClip);
    }
    public void playBlockFellDownSound()
    {
        if (blockFellDownSoundAvailable)
        {
            audioSource.PlayOneShot(blockFellDownSoundClip);
            
            blockFellDownSoundAvailable = false;
            StartCoroutine(makeBlockFellDownSoundAvailable(0.051f));
        }
    }
    public void playRocketShootedSound()
    {
        audioSource.PlayOneShot(rocketShootedSoundClip);
    }
    public void playRocketHitSound()
    {
        audioSource.PlayOneShot(rocketHitSoundClip);
    }
    
    public void playBoxDestroyedSound()
    {
        audioSource.PlayOneShot(boxDestroyedSoundClip);
    }
    public void playStoneDestroyedSound()
    {
        audioSource.PlayOneShot(stoneDestroyedSoundClip);
    }
    public void playVaseCrackedSound()
    {
        audioSource.PlayOneShot(vaseCrackedSoundClip);
    }
    public void playVaseDestroyedSound()
    {
        audioSource.PlayOneShot(vaseDestroyedSoundClip);
    }
    
    
    public void playGameWonSoundInvoke(float delay)
    {
        Invoke("playGameWonSound", delay);
    }
    public void playGameOverSoundInvoke(float delay)
    {
        Invoke("playGameOverSound", delay);
    }
    public void playGameWonSound()
    {
        audioSource.PlayOneShot(gameWonSoundClip);
    }
    public void playGameOverSound()
    {
        audioSource.PlayOneShot(gameOverSoundClip);
    }

    IEnumerator makeBlockFellDownSoundAvailable(float time)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;

            if (timer >= time)
            {
                blockFellDownSoundAvailable = true;
                yield break;
            }
            
            yield return null;
        }
    }
}
