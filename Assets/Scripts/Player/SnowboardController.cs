using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SnowboardController : NetworkBehaviour
{
    [SerializeField] private Transform frontPoint;
    [SerializeField] private float acceleration = 500f; // �leri/Geri ba�lang�� kuvveti
    [SerializeField] private float maxAcceleration = 2000f; // Maksimum ileri kuvvet
    [SerializeField] private float steering = 100f; // D�n�� h�z�
    [SerializeField] private float maxSpeed = 30f; // Maksimum h�z
    [SerializeField] private float drag = 0.98f; // H�z� azaltmak i�in s�r�klenme
    [SerializeField] private float throttleIncreaseRate = 2f; // H�zlanma fakt�r� art�� h�z�
    [SerializeField] private float throttleDecreaseRate = 5f; // H�zlanma fakt�r� azalma h�z�
    [SerializeField] private float brakeForce = 0.2f;
    [SerializeField] private float minDrag = 0.1f;
    [SerializeField] private float maxDrag = 1f;
    [SerializeField] private float gravityScale;
    [SerializeField] private float slopeSlideStrength = 1;
    [SerializeField] private GameObject CameraPrefab;
    [SerializeField] private GameObject spawnObjectPrefab;

    public float groundCheckDistance = 0.2f; // Yerden mesafeyi kontrol etmek i�in raycast mesafesi
    public LayerMask groundLayer;           // Yaln�zca zemin katman�n� kontrol etmek i�in
    private bool isGrounded = false;        // Yerle temas durumunu saklar

    private bool controlsEnabled = true;
    private float throttle = 0f; // H�zlanma fakt�r�
    private GameObject playerCamera;
    private Rigidbody rb; // RigidBody referans�
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
            playerCamera = Instantiate(CameraPrefab); // Kamera prefab'�n� olu�tur
            playerCamera.GetComponent<CameraFollow>().SetTarget(transform); // Player'� takip et
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
        // 1. Kullan�c� girdilerini al
        moveInput = Input.GetAxis("Vertical"); // �leri/Geri hareket
        turnInput = Input.GetAxis("Horizontal"); // Sa�a/Sola d�n��

        if (IsLocalPlayer && Input.GetKeyDown(KeyCode.F) && !Race_Manager.Instance.isRaceActive)
        {
            if (Race_Manager.Instance == null )
            {
                Debug.LogError("RaceManager.Instance null! RaceManager sahneye do�ru eklenmemi� olabilir.");
                return;
            }
            Debug.Log("F tu�una bas�ld�!");
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

        // 2. H�zlanma fakt�r�n� kontrol et
        if (moveInput > 0 && isGrounded) // Sadece yerdeyken h�zlanma art�r�l�r
        {
            throttle += throttleIncreaseRate * Time.fixedDeltaTime;
        }
        else if (moveInput == 0 && isGrounded) // Fren yapmad�ysak, h�z d����� yava� olur
        {
            throttle -= throttleDecreaseRate * 0.5f * Time.fixedDeltaTime;
        }
        else
        {
            throttle -= throttleDecreaseRate * Time.fixedDeltaTime;
        }

        throttle = Mathf.Clamp(throttle, 0f, 1f); // H�zlanma fakt�r�n� 0 ile 1 aras�nda s�n�rla

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
        // 1. Raycast y�ntemi
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            return true;
        }

        // 2. Collider y�ntemi
        return false; // E�er raycast bir �ey bulamazsa false d�nd�r
    }


    private void Rigidbody()
    {
        // 5. Yer�ekimi etkisi
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * gravityScale * rb.mass, ForceMode.Acceleration); // Yer�ekimi etkisi
        }
    }

    private void Turn()
    {
        // 4. Sa�a/Sola d�n��
        float turn = turnInput * steering * Time.fixedDeltaTime;
        transform.Rotate(0, turn, 0);
    }

    private void Dragging(float MoveInput)
    {
        // 6. S�r�klenme (Drag)
        if (isGrounded && MoveInput == 0)
        {
            rb.velocity *= Mathf.Lerp(1f, drag, Time.fixedDeltaTime); // Daha yumu�ak yava�lama
        }

        // 6. Dinamik S�r�klenme (Drag)
        float dynamicDrag = Mathf.Lerp(maxDrag, minDrag, rb.velocity.magnitude / maxSpeed); // H�za ba�l� drag
        rb.velocity *= Mathf.Clamp01(1f - dynamicDrag * Time.fixedDeltaTime);
    }

    private void MovementByFrontPoint()
    {
        // 3. �n nokta y�n�ne g�re hareket kuvvetini uygula
        if (frontPoint != null) // �n nokta transformunu kontrol et
        {
            Vector3 directionToMove = (frontPoint.position - transform.position).normalized; // �n noktaya do�ru y�n
            float currentAcceleration = Mathf.Lerp(0, maxAcceleration, throttle); // H�zlanmay� hesaba kat
            Vector3 forwardForce = directionToMove * moveInput * currentAcceleration * Time.fixedDeltaTime;

            // Maksimum h�z s�n�r�n� kontrol et
            if (rb.velocity.magnitude < maxSpeed || moveInput == 0) // Bo�ta kalsa da kayma olur
            {
                rb.AddForce(forwardForce, ForceMode.Acceleration);
            }
        }
    }

    private void Somersault()
    {
        // 7. Ters d�nme durumu
        if (Vector3.Dot(transform.up, Vector3.up) < 0.5f && CheckGround()) // Araba yakla��k olarak ters d�nm��se
        {
            // Kullan�c� belirli bir tu�a bast���nda takla atmay� tetikle
            if (Input.GetKeyDown(KeyCode.LeftShift)) // "R" tu�u d�zelme i�in �rnek
            {
                // Ters d�nmeyi d�zeltmek i�in yukar� do�ru bir kuvvet uygula
                rb.AddForce(Vector3.up * 10f, ForceMode.Impulse); // Z�plama kuvveti
                rb.AddTorque(transform.right * 30f, ForceMode.Impulse); // Takla i�in tork kuvveti
            }
        }
    }

    private void Slope()
    {
        // 8. E�ime G�re Kayma
        if (isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
            {
                Vector3 groundNormal = hit.normal; // Zeminin normal vekt�r�
                Vector3 slopeDirection = Vector3.Cross(transform.right, groundNormal); // E�im y�n�

                float slopeAngle = Vector3.Angle(groundNormal, Vector3.up); // E�imin a��s�
                if (slopeAngle > 0.1f) // Hafif bir e�im varsa bile kayma sa�la
                {
                    float slopeFactor = Mathf.Clamp01(slopeAngle / 45f); // 45 derece e�imde maksimum kayma
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
                if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f) // Y�zey yukar� do�ruysa
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
            Destroy(playerCamera); // Oyuncu ayr�ld���nda kameray� yok et
        }
    }
}

