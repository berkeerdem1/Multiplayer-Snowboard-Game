using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    private AudioSource _audio;
    [SerializeField] private AudioClip _startAudio, _background;

    public bool isInGame = false;

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

    IEnumerator Coroutine()
    {
        _audio.clip = _startAudio;
        _audio.volume = 0.4f;
        _audio.loop = false; 
        _audio.Play();

        yield return new WaitForSeconds(_startAudio.length);

        _audio.clip = _background;
        _audio.loop = true; 
        _audio.volume = 0.2f;
        _audio.Play();
    }
}
