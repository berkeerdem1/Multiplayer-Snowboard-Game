using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowboardManager : MonoBehaviour
{
    public Transform frontPoint;
    public float acceleration = 500f; // Ýleri/Geri baþlangýç kuvveti
    public float maxAcceleration = 2000f; // Maksimum ileri kuvvet
    public float steering = 100f; // Dönüþ hýzý
    public float maxSpeed = 30f; // Maksimum hýz
    public float drag = 0.98f; // Hýzý azaltmak için sürüklenme
    public float throttleIncreaseRate = 2f; // Hýzlanma faktörü artýþ hýzý
    public float throttleDecreaseRate = 5f; // Hýzlanma faktörü azalma hýzý
    public float brakeForce = 0.2f;
    public bool isGrounded = false;


    private Rigidbody rb; // RigidBody referansý
    private float throttle = 0f; // Hýzlanma faktörü

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        InvokeRepeating("Movement", 0, 0.02f);
    }

    private void Movement()
    {
        // 1. Kullanýcý girdilerini al
        float moveInput = Input.GetAxis("Vertical"); // Ýleri/Geri hareket
        float turnInput = Input.GetAxis("Horizontal"); // Saða/Sola dönüþ

        // 2. Hýzlanma faktörünü kontrol et
        if (moveInput > 0)
        {
            // Ýleri hareket sýrasýnda hýzlanmayý artýr
            throttle += throttleIncreaseRate * Time.fixedDeltaTime;
        }
        else
        {
            // Geri hareket veya durma sýrasýnda hýzlanmayý azalt
            throttle -= throttleDecreaseRate * Time.fixedDeltaTime;
        }
        throttle = Mathf.Clamp(throttle, 0f, 1f); // Hýzlanma faktörünü 0 ile 1 arasýnda sýnýrla

        // 3. Ön nokta yönüne göre hareket kuvvetini uygula
        if (frontPoint != null) // Ön nokta transformunu kontrol et
        {
            Vector3 directionToMove = (frontPoint.position - transform.position).normalized; // Ön noktaya doðru yön
            float currentAcceleration = Mathf.Lerp(acceleration, maxAcceleration, throttle); // Hýzlanmayý hesaba kat
            Vector3 forwardForce = directionToMove * moveInput * currentAcceleration * Time.fixedDeltaTime;

            // Maksimum hýz sýnýrýný kontrol et
            if (rb.velocity.magnitude < maxSpeed)
            {
                rb.AddForce(forwardForce, ForceMode.Acceleration);
            }
        }

        // 4. Saða/Sola dönüþ
        float turn = turnInput * steering * Time.fixedDeltaTime;
        transform.Rotate(0, turn, 0);

        // 5. Sürüklenme efekti
        rb.velocity *= drag;

        if (Input.GetKey(KeyCode.Space))
        {
            rb.velocity *= (1f - brakeForce * Time.fixedDeltaTime);
        }

        if (Vector3.Dot(transform.up, Vector3.up) < 0.5f && isGrounded) // Araba yaklaþýk olarak ters dönmüþse
        {
            // Kullanýcý belirli bir tuþa bastýðýnda takla atmayý tetikle
            if (Input.GetKeyDown(KeyCode.LeftShift)) // "R" tuþu düzelme için örnek
            {
                // Ters dönmeyi düzeltmek için yukarý doðru bir kuvvet uygula
                rb.AddForce(Vector3.up * 10f, ForceMode.Impulse); // Zýplama kuvveti
                rb.AddTorque(transform.right * 30f, ForceMode.Impulse); // Takla için tork kuvveti
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Yüzeyin normaline bakarak yere temas kontrolü
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
        isGrounded = false;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false; // Çarpýþma sona erdiðinde yere temas kesilir
    }
}

