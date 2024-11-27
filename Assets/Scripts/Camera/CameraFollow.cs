using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Transform target; // Takip edilecek arac
    public Vector3 offset = new Vector3(0, 5, -10); // Kameran�n arabaya g�re sabit pozisyonu
    public float followSpeed = 10f; // Pozisyon takip h�z�
    public float heightLock = 5f; // Kameran�n sabit y�kseklik de�eri
    private SnowboardController snowboard;
    public float smoothness = 0.125f; // Pozisyon ge�i� yumu�atma fakt�r�

    float playerGroundTimer = 0f;
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
            //    target = potentialTarget.transform; // Bulunan hedefi atay�n
            //}
            //else
            //{
            //    return; // Hedef bulunamazsa ��k
            //}
            return;
        }

        Vector3 desiredPosition ;

        if (target.GetComponent<SnowboardController>().SetFlippingState)
        {
            desiredPosition = target.position + offset;
        }
        else
        {
            desiredPosition = target.position + target.rotation * offset;
        }

        if (!target.GetComponent<SnowboardController>().CheckGround())
        {
            playerGroundTimer += Time.deltaTime;
            if (playerGroundTimer >= 2f)
            {
                desiredPosition = target.position + offset;
            }
        }
        else
        {
            playerGroundTimer = 0f;
            desiredPosition = target.position + target.rotation * offset;
        }


        // Kameran�n hedef pozisyonunu hesapla
        

        // Y�kseklik sabitleme kontrol�
        // E�er araba al�alm�yor, y�kselmiyor veya ters d�nmemi�se, kameran�n y�ksekli�ini sabit tut
        //if (Vector3.Dot(target.up, Vector3.up) < 0.5f) // Araba ters d�nm��se
        //{
        //    desiredPosition.y = heightLock; // Sabit y�ksekli�i uygula
        //}
        //else
        //{
        //    // Araban�n pozisyonuna dinamik olarak uyum sa�lar
        //    desiredPosition.y = Mathf.Lerp(transform.position.y, target.position.y + offset.y, followSpeed * Time.deltaTime);
        //}

        desiredPosition.y = Mathf.Lerp(transform.position.y, target.position.y + offset.y, followSpeed * Time.deltaTime);
        // Kameray� hedef pozisyona yumu�ak�a hareket ettir
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Kameran�n arabay� izledi�i a��y� koru
        Vector3 lookAtPosition = target.position;
        lookAtPosition.y = transform.position.y; // Y ekseninde kameray� sabit tutar
        transform.LookAt(lookAtPosition);

       
    }
}
