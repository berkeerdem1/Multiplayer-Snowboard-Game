using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SnowboardController : NetworkBehaviour
{
    [SerializeField] private Transform frontPoint;
    [SerializeField] private float acceleration = 500f; // Ýleri/Geri baþlangýç kuvveti
    [SerializeField] private float maxAcceleration = 2000f; // Maksimum ileri kuvvet
    [SerializeField] private float steering = 100f; // Dönüþ hýzý
    [SerializeField] private float maxSpeed = 30f; // Maksimum hýz
    [SerializeField] private float drag = 0.98f; // Hýzý azaltmak için sürüklenme
    [SerializeField] private float throttleIncreaseRate = 2f; // Hýzlanma faktörü artýþ hýzý
    [SerializeField] private float throttleDecreaseRate = 5f; // Hýzlanma faktörü azalma hýzý
    [SerializeField] private float brakeForce = 0.2f;
    [SerializeField] private float minDrag = 0.1f;
    [SerializeField] private float maxDrag = 1f;
    [SerializeField] private float gravityScale;
    [SerializeField] private float slopeSlideStrength = 1;
    [SerializeField] private GameObject CameraPrefab;
    [SerializeField] private GameObject spawnObjectPrefab;

    public float groundCheckDistance = 0.2f; // Yerden mesafeyi kontrol etmek için raycast mesafesi
    public LayerMask groundLayer;           // Yalnýzca zemin katmanýný kontrol etmek için
    private bool isGrounded = false;        // Yerle temas durumunu saklar

    private bool controlsEnabled = true;
    private float throttle = 0f; // Hýzlanma faktörü
    private GameObject playerCamera;
    private Rigidbody rb; // RigidBody referansý
    private GameObject spawnObject;
    private TrailRenderer trail;

    private NetworkVariable<float> randomFloatNumber = new NetworkVariable<float>(5.5f, 
                                                                                NetworkVariableReadPermission.Everyone,
                                                                                NetworkVariableWritePermission.Owner);
    float moveInput, turnInput;

    public override void OnNetworkSpawn() // Netcode'un start'i
    {
        if (IsOwner)
        {
            playerCamera = Instantiate(CameraPrefab); // Kamera prefab'ýný oluþtur
            playerCamera.GetComponent<CameraFollow>().SetTarget(transform); // Player'ý takip et
        }
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //trail = GetComponentInChildren<TrailRenderer>();
        //trail.startWidth = 2f;
        InvokeRepeating("Movement", 0, 0.02f);
    }

    private void Update()
    {
        // 1. Kullanýcý girdilerini al
        moveInput = Input.GetAxis("Vertical"); // Ýleri/Geri hareket
        turnInput = Input.GetAxis("Horizontal"); // Saða/Sola dönüþ

        if (IsLocalPlayer && Input.GetKeyDown(KeyCode.F) && !Race_Manager.Instance.isRaceActive)
        {
            if (Race_Manager.Instance == null )
            {
                Debug.LogError("RaceManager.Instance null! RaceManager sahneye doðru eklenmemiþ olabilir.");
                return;
            }
            Debug.Log("F tuþuna basýldý!");
            Race_Manager.Instance.PlayerReadyServerRpc(NetworkManager.Singleton.LocalClientId);
            OnPlayerPressF();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity *= (1f - brakeForce * Time.fixedDeltaTime);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            rb.velocity *= (1f - brakeForce * Time.fixedDeltaTime);
        }

        ////if (IsMoving() && CheckGround()) // Snowboard hareket ediyorsa izi aktif et
        ////{
        ////    trail.emitting = true;
        ////}
        ////else // Snowboard duruyorsa izi kapat
        ////{
        ////    trail.emitting = false;
        ////}
    }
    bool IsMoving()
    {
        return Mathf.Abs(rb.velocity.magnitude) > 0.1f;
    }

    public void DisableControls()
    {
        controlsEnabled = false;
    }

    public void EnableControls()
    {
        controlsEnabled = true;
    }

    private void Movement()
    {
        if (!IsOwner) return;

        if (!controlsEnabled)
        {
            Turn();
            return; 
        }

        // 2. Hýzlanma faktörünü kontrol et
        if (moveInput > 0 && isGrounded) // Sadece yerdeyken hýzlanma artýrýlýr
        {
            throttle += throttleIncreaseRate * Time.fixedDeltaTime;
        }
        else if (moveInput == 0 && isGrounded) // Fren yapmadýysak, hýz düþüþü yavaþ olur
        {
            throttle -= throttleDecreaseRate * 0.5f * Time.fixedDeltaTime;
        }
        else
        {
            throttle -= throttleDecreaseRate * Time.fixedDeltaTime;
        }

        throttle = Mathf.Clamp(throttle, 0f, 1f); // Hýzlanma faktörünü 0 ile 1 arasýnda sýnýrla

        //Debug.Log($"Current Velocity: {rb.velocity.magnitude}");

        Turn();
        Rigidbody();
        //Dragging(moveInput);
        MovementByFrontPoint();
        Somersault();
        Slope();
        CheckGround();

    }

    public void OnPlayerPressF()
    {
        var raceManager = FindObjectOfType<Race_Manager>();
        raceManager.AddPlayerToRace(NetworkManager.Singleton.LocalClientId);
    }

    public bool CheckGround()
    {
        // 1. Raycast yöntemi
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            return true;
        }

        // 2. Collider yöntemi
        return false; // Eðer raycast bir þey bulamazsa false döndür
    }


    private void Rigidbody()
    {
        // 5. Yerçekimi etkisi
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * gravityScale * rb.mass, ForceMode.Acceleration); // Yerçekimi etkisi
        }
    }

    private void Turn()
    {
        // 4. Saða/Sola dönüþ
        float turn = turnInput * steering * Time.fixedDeltaTime;
        transform.Rotate(0, turn, 0);
    }

    private void Dragging(float MoveInput)
    {
        // 6. Sürüklenme (Drag)
        if (isGrounded && MoveInput == 0)
        {
            rb.velocity *= Mathf.Lerp(1f, drag, Time.fixedDeltaTime); // Daha yumuþak yavaþlama
        }

        // 6. Dinamik Sürüklenme (Drag)
        float dynamicDrag = Mathf.Lerp(maxDrag, minDrag, rb.velocity.magnitude / maxSpeed); // Hýza baðlý drag
        rb.velocity *= Mathf.Clamp01(1f - dynamicDrag * Time.fixedDeltaTime);
    }

    private void MovementByFrontPoint()
    {
        // 3. Ön nokta yönüne göre hareket kuvvetini uygula
        if (frontPoint != null) // Ön nokta transformunu kontrol et
        {
            Vector3 directionToMove = (frontPoint.position - transform.position).normalized; // Ön noktaya doðru yön
            float currentAcceleration = Mathf.Lerp(0, maxAcceleration, throttle); // Hýzlanmayý hesaba kat
            Vector3 forwardForce = directionToMove * moveInput * currentAcceleration * Time.fixedDeltaTime;

            // Maksimum hýz sýnýrýný kontrol et
            if (rb.velocity.magnitude < maxSpeed || moveInput == 0) // Boþta kalsa da kayma olur
            {
                rb.AddForce(forwardForce, ForceMode.Acceleration);
            }
        }
    }

    private void Somersault()
    {
        // 7. Ters dönme durumu
        if (Vector3.Dot(transform.up, Vector3.up) < 0.5f && CheckGround()) // Araba yaklaþýk olarak ters dönmüþse
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

    private void Slope()
    {
        // 8. Eðime Göre Kayma
        if (isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
            {
                Vector3 groundNormal = hit.normal; // Zeminin normal vektörü
                Vector3 slopeDirection = Vector3.Cross(transform.right, groundNormal); // Eðim yönü

                float slopeAngle = Vector3.Angle(groundNormal, Vector3.up); // Eðimin açýsý
                if (slopeAngle > 0.1f) // Hafif bir eðim varsa bile kayma saðla
                {
                    float slopeFactor = Mathf.Clamp01(slopeAngle / 45f); // 45 derece eðimde maksimum kayma
                    Vector3 slopeForce = slopeDirection.normalized * slopeFactor * slopeSlideStrength;

                    rb.AddForce(slopeForce, ForceMode.Acceleration);
                }
            }
        }
    }


    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f) // Yüzey yukarý doðruysa
                {
                    isGrounded = true;
                    return;
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }

    [ServerRpc]
    private void TestingServerRpc()  // Host sahibiyse konsola yazar
    {
        Debug.Log(OwnerClientId + " Testing Server Rpc");
    }

    [ClientRpc]
    private void TestingClientRpc() // Client'se konsola yazar
    {
        Debug.Log(OwnerClientId + " Testing Client Rpc");
    }

    private void OnDestroy()
    {
        if (IsOwner && playerCamera != null)
        {
            Destroy(playerCamera); // Oyuncu ayrýldýðýnda kamerayý yok et
        }
    }
}

