using UnityEngine;

public class ShotgunAnimationAudio : MonoBehaviour
{
    public AudioSource rackAudio;
    
    //play rack sound
    public void PlayRackSound()
    {
        rackAudio.Stop();
        rackAudio.Play();
    }
}
