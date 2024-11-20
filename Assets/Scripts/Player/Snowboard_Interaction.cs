using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snowboard_Interaction : MonoBehaviour
{
    public Transform player; // Player objesi (Prefab içinde snowboard'un çocuðu)
    public Vector3 playerDefaultLocalPosition = new Vector3(0, 0.5f, 0); // Player'ýn snowboard üzerindeki varsayýlan yerel pozisyonu
    public Quaternion playerDefaultLocalRotation = Quaternion.identity; // Player'ýn varsayýlan yerel rotasyonu
    public float rotationLerpSpeed = 5f; // Rotasyon için lerp hýzý
    public Vector3 playerOffset = new Vector3(0, 0.5f, 0); // Player'ýn snowboard'a göre baþlangýç pozisyonu

    private SnowboardController controller;

    private void Awake()
    {
        controller = GetComponent<SnowboardController>();
    }

    private void FixedUpdate()
    {
        if (player != null)
        {
            // 1. Pozisyonu snowboard'un merkezine göre hizala
            player.position = transform.TransformPoint(playerOffset);

            // 2. Rotasyonu snowboard'un rotasyonuna göre hizala
            Quaternion targetRotation = transform.rotation;
            player.rotation = Quaternion.Lerp(player.rotation, targetRotation, Time.fixedDeltaTime * rotationLerpSpeed);
        }
    }
}
