using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Takip edilecek araç
    public Vector3 offset = new Vector3(0, 5, -10); // Kameranýn arabaya göre sabit pozisyonu
    public float followSpeed = 10f; // Pozisyon takip hýzý
    public float heightLock = 5f; // Kameranýn sabit yükseklik deðeri
    private SnowboardManager snowboard;

    private void Awake()
    {
        snowboard = new SnowboardManager();
    }
    private void Start()
    {
        Application.targetFrameRate = -1;
    }
    private void LateUpdate()
    {
        if (target == null) return;

        // Kameranýn hedef pozisyonunu hesapla
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Yükseklik sabitleme kontrolü
        // Eðer araba alçalmýyor, yükselmiyor veya ters dönmemiþse, kameranýn yüksekliðini sabit tut
        if (Vector3.Dot(target.up, Vector3.up) < 0.5f && snowboard.isGrounded) // Araba ters dönmüþse
        {
            desiredPosition.y = heightLock; // Sabit yüksekliði uygula
        }
        else
        {
            // Arabanýn pozisyonuna dinamik olarak uyum saðlar
            desiredPosition.y = Mathf.Lerp(transform.position.y, target.position.y + offset.y, followSpeed * Time.deltaTime);
        }

        // Kamerayý hedef pozisyona yumuþakça hareket ettir
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Kameranýn arabayý izlediði açýyý koru
        Vector3 lookAtPosition = target.position;
        lookAtPosition.y = transform.position.y; // Y ekseninde kamerayý sabit tutar
        transform.LookAt(lookAtPosition);
    }
}
