using System.Collections;
using UnityEngine;

public class Snowboard_Audios : MonoBehaviour
{
    [SerializeField] private float volumeChangeRate = 2f; // Rate of change of sound level
    [SerializeField] private float maxVolume = 1f;

    private float currentVolume;

    private SnowboardController _mysnowboard;
    private AudioSource _audio;

    private void Awake()
    {
        _mysnowboard = GetComponent<SnowboardController>();
        _audio = GetComponent<AudioSource>();
    }

    void Start()
    {
        InvokeRepeating("PlaySkiingSound", 0.5f, 0.02f);
    }

    private void PlaySkiingSound()
    {
        if (_audio == null) return;

        if (!_mysnowboard.CheckGround())
        {
            currentVolume = Mathf.Lerp(currentVolume, 0f, Time.deltaTime * 1f);
        }

        else
        {
            // Adjust volume with throttle while on the ground
            currentVolume = Mathf.Lerp(currentVolume, Mathf.Lerp(0f, maxVolume, _mysnowboard.throttle), Time.deltaTime * 1f);
        }

        if (_audio == null) return;

        if (_mysnowboard.throttle > 0)
        {
            // Relate sound level to throttle
            _audio.volume = Mathf.Lerp(_audio.volume, _mysnowboard.throttle * maxVolume, volumeChangeRate * Time.deltaTime);

            if (!_audio.isPlaying)
            {
                _audio.Play();
            }
        }

        else
        {
            // Decrease the volume gradually and stop at zero
            _audio.volume = Mathf.Lerp(_audio.volume, 0f, volumeChangeRate * Time.deltaTime);
            if (_audio.volume <= 0.01f)
            {
                _audio.Stop();
            }
        }
    }

    public void PlayTemporarySound(AudioClip tempClip)
    {
        if (_audio == null || tempClip == null) return;

        // Backup current audio
        AudioClip originalClip = _audio.clip;
        bool wasPlaying = _audio.isPlaying;

        _audio.Stop();

        _audio.PlayOneShot(tempClip);

        // Wait for the length of the temporary audio and restore the old audio      
        StartCoroutine(RestoreOriginalSound(originalClip, wasPlaying, tempClip.length));
    }

    private IEnumerator RestoreOriginalSound(AudioClip originalClip, bool wasPlaying, float delay)
    {
        // Wait for the duration of the temporary sound
        yield return new WaitForSeconds(delay);

        // Restore original audio
        _audio.clip = originalClip;

        // If the old audio is playing, continue it
        if (wasPlaying && originalClip != null)
        {
            _audio.Play();
        }
    }
}
