using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isInGame = false; // In the game control

    [SerializeField] private AudioClip _startAudio, _background;

    private AudioSource _audio;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        StartCoroutine(Coroutine());
    }

    IEnumerator Coroutine() // Main menu start music and Background music control
    {
        _audio.clip = _startAudio;
        _audio.volume = 0.4f;
        _audio.loop = false; 
        _audio.Play();

        yield return new WaitForSeconds(_startAudio.length);

        _audio.clip = _background;
        _audio.loop = true; 
        _audio.volume = 0.15f;
        _audio.Play();
    }
}
