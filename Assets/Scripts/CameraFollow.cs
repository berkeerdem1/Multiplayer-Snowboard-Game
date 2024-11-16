using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Takip edilecek ara�
    public Vector3 offset = new Vector3(0, 5, -10); // Kameran�n arabaya g�re sabit pozisyonu
    public float followSpeed = 10f; // Pozisyon takip h�z�
    public float heightLock = 5f; // Kameran�n sabit y�kseklik de�eri
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

        // Kameran�n hedef pozisyonunu hesapla
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Y�kseklik sabitleme kontrol�
        // E�er araba al�alm�yor, y�kselmiyor veya ters d�nmemi�se, kameran�n y�ksekli�ini sabit tut
        if (Vector3.Dot(target.up, Vector3.up) < 0.5f && snowboard.isGrounded) // Araba ters d�nm��se
        {
            desiredPosition.y = heightLock; // Sabit y�ksekli�i uygula
        }
        else
        {
            // Araban�n pozisyonuna dinamik olarak uyum sa�lar
            desiredPosition.y = Mathf.Lerp(transform.position.y, target.position.y + offset.y, followSpeed * Time.deltaTime);
        }

        // Kameray� hedef pozisyona yumu�ak�a hareket ettir
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Kameran�n arabay� izledi�i a��y� koru
        Vector3 lookAtPosition = target.position;
        lookAtPosition.y = transform.position.y; // Y ekseninde kameray� sabit tutar
        transform.LookAt(lookAtPosition);
    }
}
