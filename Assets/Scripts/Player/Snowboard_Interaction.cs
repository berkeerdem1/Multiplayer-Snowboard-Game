using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snowboard_Interaction : MonoBehaviour
{
    public Transform player; // Player objesi (Prefab i�inde snowboard'un �ocu�u)
    public Vector3 playerDefaultLocalPosition = new Vector3(0, 0.5f, 0); // Player'�n snowboard �zerindeki varsay�lan yerel pozisyonu
    public Quaternion playerDefaultLocalRotation = Quaternion.identity; // Player'�n varsay�lan yerel rotasyonu
    public float rotationLerpSpeed = 5f; // Rotasyon i�in lerp h�z�
    public Vector3 playerOffset = new Vector3(0, 0.5f, 0); // Player'�n snowboard'a g�re ba�lang�� pozisyonu
    public Vector3 playerShieldOffset = new Vector3(0, 1f, 0); // Player'�n snowboard'a g�re ba�lang�� pozisyonu

    [SerializeField] private Transform shieldPos;

    private SnowboardController controller;

    private void Awake()
    {
        controller = GetComponent<SnowboardController>();
    }

    private void FixedUpdate()
    {
        if (player != null)
        {
            // 1. Pozisyonu snowboard'un merkezine g�re hizala
            player.position = transform.TransformPoint(playerOffset);
            shieldPos.position = transform.TransformPoint(playerShieldOffset);

            // 2. Rotasyonu snowboard'un rotasyonuna g�re hizala
            Quaternion targetRotation = transform.rotation;
            player.rotation = Quaternion.Lerp(player.rotation, targetRotation, Time.fixedDeltaTime * rotationLerpSpeed);
            shieldPos.rotation = Quaternion.Lerp(shieldPos.rotation, targetRotation, Time.fixedDeltaTime * rotationLerpSpeed);

        }
    }
}
