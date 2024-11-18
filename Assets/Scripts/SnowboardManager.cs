using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SnowboardManager : NetworkBehaviour
{
    public Transform frontPoint;
    public float acceleration = 500f; // �leri/Geri ba�lang�� kuvveti
    public float maxAcceleration = 2000f; // Maksimum ileri kuvvet
    public float steering = 100f; // D�n�� h�z�
    public float maxSpeed = 30f; // Maksimum h�z
    public float drag = 0.98f; // H�z� azaltmak i�in s�r�klenme
    public float throttleIncreaseRate = 2f; // H�zlanma fakt�r� art�� h�z�
    public float throttleDecreaseRate = 5f; // H�zlanma fakt�r� azalma h�z�
    public float brakeForce = 0.2f;
    public bool isGrounded = false;

    [SerializeField] private GameObject CameraPrefab;
    private GameObject playerCamera;
    private Rigidbody rb; // RigidBody referans�
    private float throttle = 0f; // H�zlanma fakt�r�


    [SerializeField] private GameObject spawnObjectPrefab;
    private GameObject spawnObject;

    private NetworkVariable<float> randomFloatNumber = new NetworkVariable<float>(5.5f, 
                                                                                NetworkVariableReadPermission.Everyone,
                                                                                NetworkVariableWritePermission.Owner);

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
        InvokeRepeating("Movement", 0, 0.02f);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            rb.velocity *= (1f - brakeForce * Time.fixedDeltaTime);
        }

        //if (Input.GetKey(KeyCode.J))
        //{
        //    if(spawnObjectPrefab != null)
        //    {
        //        spawnObject = Instantiate(spawnObjectPrefab);
        //        spawnObject.GetComponent<NetworkObject>().Spawn();  // Netcode'da obje spawnlar                                                                
        //    }
        //    //TestingServerRpc();
        //    //randomFloatNumber.Value = Random.Range(0, 100);
        //}

        //if (Input.GetKey(KeyCode.G))
        //{
        //    if (spawnObjectPrefab != null)
        //    {
        //        spawnObject.GetComponent<NetworkObject>().Despawn();  // Netcode'da spawnlanan objeyi yok eder
        //        Destroy(spawnObject);
        //    }
        //    //TestingServerRpc();
        //    //randomFloatNumber.Value = Random.Range(0, 100);
        //}
    }

    private void Movement()
    {
        if (!IsOwner)
            return;

        // 1. Kullan�c� girdilerini al
        float moveInput = Input.GetAxis("Vertical"); // �leri/Geri hareket
        float turnInput = Input.GetAxis("Horizontal"); // Sa�a/Sola d�n��

        // 2. H�zlanma fakt�r�n� kontrol et
        if (moveInput > 0)
        {
            // �leri hareket s�ras�nda h�zlanmay� art�r
            throttle += throttleIncreaseRate * Time.fixedDeltaTime;
        }
        else
        {
            // Geri hareket veya durma s�ras�nda h�zlanmay� azalt
            throttle -= throttleDecreaseRate * Time.fixedDeltaTime;
        }
        throttle = Mathf.Clamp(throttle, 0f, 1f); // H�zlanma fakt�r�n� 0 ile 1 aras�nda s�n�rla

        // 3. �n nokta y�n�ne g�re hareket kuvvetini uygula
        if (frontPoint != null) // �n nokta transformunu kontrol et
        {
            Vector3 directionToMove = (frontPoint.position - transform.position).normalized; // �n noktaya do�ru y�n
            float currentAcceleration = Mathf.Lerp(acceleration, maxAcceleration, throttle); // H�zlanmay� hesaba kat
            Vector3 forwardForce = directionToMove * moveInput * currentAcceleration * Time.fixedDeltaTime;

            // Maksimum h�z s�n�r�n� kontrol et
            if (rb.velocity.magnitude < maxSpeed)
            {
                rb.AddForce(forwardForce, ForceMode.Acceleration);
            }
        }

        // 4. Sa�a/Sola d�n��
        float turn = turnInput * steering * Time.fixedDeltaTime;
        transform.Rotate(0, turn, 0);

        // 5. S�r�klenme efekti
        rb.velocity *= drag;

        

        if (Vector3.Dot(transform.up, Vector3.up) < 0.5f && isGrounded) // Araba yakla��k olarak ters d�nm��se
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

    void OnCollisionStay(Collision collision)
    {
        // Y�zeyin normaline bakarak yere temas kontrol�
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
        isGrounded = false; // �arp��ma sona erdi�inde yere temas kesilir
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

