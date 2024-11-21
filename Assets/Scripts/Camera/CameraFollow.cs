using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Transform target; // Takip edilecek arac
    public Vector3 offset = new Vector3(0, 5, -10); // Kameranýn arabaya göre sabit pozisyonu
    public float followSpeed = 10f; // Pozisyon takip hýzý
    public float heightLock = 5f; // Kameranýn sabit yükseklik deðeri
    private SnowboardController snowboard;

    private void Awake()
    {
        snowboard = new SnowboardController();
    }
    private void Start()
    {
        Application.targetFrameRate = -1;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if(target == null)
        {
            //GameObject potentialTarget = GameObject.Find("Snowboard(Clone)"); // "Player" tagine sahip objeyi ara
            //if (potentialTarget != null)
            //{
            //    target = potentialTarget.transform; // Bulunan hedefi atayýn
            //}
            //else
            //{
            //    return; // Hedef bulunamazsa çýk
            //}
            return;
        }
        

        // Kameranýn hedef pozisyonunu hesapla
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Yükseklik sabitleme kontrolü
        // Eðer araba alçalmýyor, yükselmiyor veya ters dönmemiþse, kameranýn yüksekliðini sabit tut
        if (Vector3.Dot(target.up, Vector3.up) < 0.5f) // Araba ters dönmüþse
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
