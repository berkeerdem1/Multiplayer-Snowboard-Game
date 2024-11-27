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
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bullerPrefab;
    [SerializeField] private float bulletSpeed;

    private Vector3[] flipDirections = new Vector3[]
    {
        Vector3.forward,  // Öne takla
        Vector3.back,     // Arkaya takla
        Vector3.left,     // Sola takla
        Vector3.right,    // Saða takla
    };


    [SerializeField] private GameObject shield;

    [SerializeField] private float dashForce = 10f; // Dash gücü
    [SerializeField] private float dashDuration = 0.2f; // Dash süresi
    private bool isDashing = false;

    [SerializeField]
    private float jumpForce = 10;

    [SerializeField] private AudioClip dashSudio, shieldActiveAudio, hurtAudio;
    [SerializeField] private AudioSource audio; // Motor sesi AudioSource
    [SerializeField] private float maxVolume = 1f;    // Maksimum ses seviyesi
    [SerializeField] private float volumeChangeRate = 2f; // Ses seviyesinin deðiþim hýzý

    [SerializeField] private float playInterval = 0.2f; // Sesin ne sýklýkta çalýnacaðý (saniye)
    private float currentVolume; // Anlýk ses seviyesi

    private float lastPlayTime; // Son ses çalma zamaný


    [SerializeField] private float groundCheckDistance = 0.2f; // Yerden mesafeyi kontrol etmek için raycast mesafesi
    [SerializeField] private LayerMask groundLayer;           // Yalnýzca zemin katmanýný kontrol etmek için
    private bool isGrounded = false;        // Yerle temas durumunu saklar

    public bool SetFlippingState = false;

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


            // Ýstemci olduðunuzu kontrol edin
            if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("Bu bir istemci.");
            }

            // Sunucuya baðlý olup olmadýðýnýzý kontrol edin
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.Log("Ýstemci baþarýyla sunucuya baðlý.");
            }
            else
            {
                Debug.LogError("Ýstemci sunucuya baðlý deðil!");
            }
        }

        if (IsServer)
        {
            Debug.Log("Bu obje sunucuda spawn oldu.");
        }
        else
        {
            Debug.Log("Bu obje istemci tarafýnda spawn oldu.");
        }
    }

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

    }
    private void Start()
    {
        //trail = GetComponentInChildren<TrailRenderer>();
        //trail.startWidth = 2f;
        shield.SetActive(false);
        InvokeRepeating("Movement", 0, 0.02f);
    }

    private void Update()
    {
        // 1. Kullanýcý girdilerini al
        moveInput = Input.GetAxis("Vertical"); // Ýleri/Geri hareket
        turnInput = Input.GetAxis("Horizontal"); // Saða/Sola dönüþ

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            rb.velocity *= (1f - brakeForce * Time.fixedDeltaTime);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            rb.velocity *= (1f - brakeForce * Time.fixedDeltaTime);
        }

        if (Input.GetKeyUp(KeyCode.Space) && isGrounded)
        {
            Jump();
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

    public void ApplyItemEffect()
    {
        // Ýtem etkisini burada uygula
        Debug.Log("Item etkisi uygulandý!");
        // Örnek: Can arttýrma
        // health += 10;
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
        Gravity();
        //Dragging(moveInput);
        MovementByFrontPoint();
        Somersault();
        Slope();
        CheckGround();
        PlaySkiingSound();

        Debug.DrawRay(firePoint.position, firePoint.forward * 5, Color.red, 2f);


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

    private void Gravity()
    {
        // 5. Yerçekimi etkisi
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * gravityScale * rb.mass, ForceMode.Acceleration); // Yerçekimi etkisi
        }
        else gravityScale = 10;
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
        if (/*Vector3.Dot(transform.up, Vector3.up) < 0.5f &&*/ CheckGround()) // Araba yaklaþýk olarak ters dönmüþse
        {
            // Kullanýcý belirli bir tuþa bastýðýnda takla atmayý tetikle
            if (Input.GetKeyDown(KeyCode.LeftShift)) // "R" tuþu düzelme için örnek
            {
                PerformRandomFlip();
            }
        }

        // Zeminle temas saðlandýðýnda takla durumunu devre dýþý býrak
        if (isGrounded)
        {
            SetFlippingState = false;
        }
        else
        {
            SetFlippingState = true;

        }
    }

    void PerformRandomFlip()
    {
        // Rastgele bir yön seç
        Vector3 randomDirection = flipDirections[Random.Range(0, flipDirections.Length)];

        // Yukarý doðru bir zýplama kuvveti uygula
        rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);

        // Rastgele bir yönde tork uygula
        rb.AddTorque(randomDirection * 20f, ForceMode.Impulse);
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

    private void Jump()
    {
        if (!IsOwner) return;

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        gravityScale = 20;
    }


    private void PlaySkiingSound()
    {
        if (audio == null) return;


        if (!isGrounded)
        {
            currentVolume = Mathf.Lerp(currentVolume, 0f, Time.deltaTime * 1f);
        }
        else
        {
            // Yerdeyken throttle ile ses seviyesi belirle
            currentVolume = Mathf.Lerp(currentVolume, Mathf.Lerp(0f, maxVolume, throttle), Time.deltaTime * 1f);
        }

        if (audio == null) return;

        if (throttle > 0)
        {
            // Ses seviyesini throttle ile iliþkilendir
            audio.volume = Mathf.Lerp(audio.volume, throttle * maxVolume, volumeChangeRate * Time.deltaTime);

            // Eðer ses çalmýyorsa baþlat
            if (!audio.isPlaying)
            {
                audio.Play();
            }
        }
        else
        {
            // Ses seviyesini kademeli olarak azalt ve sýfýr olduðunda durdur
            audio.volume = Mathf.Lerp(audio.volume, 0f, volumeChangeRate * Time.deltaTime);
            if (audio.volume <= 0.01f)
            {
                audio.Stop();
            }
        }

    }

    public void PlayTemporarySound(AudioClip tempClip)
    {
        if (audio == null || tempClip == null) return;

        // Mevcut sesi yedekle
        AudioClip originalClip = audio.clip;
        bool wasPlaying = audio.isPlaying;

        // Çalan sesi durdur
        audio.Stop();

        // PlayOneShot ile geçici sesi çal
        audio.PlayOneShot(tempClip);

        // Geçici sesin uzunluðu kadar bekleyip eski sesi geri yükle
        StartCoroutine(RestoreOriginalSound(originalClip, wasPlaying, tempClip.length));
    }

    private IEnumerator RestoreOriginalSound(AudioClip originalClip, bool wasPlaying, float delay)
    {
        // Geçici sesin süresi kadar bekle
        yield return new WaitForSeconds(delay);

        // Orijinal sesi geri yükle
        audio.clip = originalClip;

        // Eðer eski ses çalýyorsa devam ettir
        if (wasPlaying && originalClip != null)
        {
            audio.Play();
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




    public void Shoot()    
    {
        // Mermiyi doðru bir rotasyonla spawnlayýn.
        //GameObject newBullet =Instantiate(bullerPrefab, firePoint.position, firePoint.rotation);
        //NetworkObject networkObject = newBullet.GetComponent<NetworkObject>();
        //networkObject.Spawn();
        NetworkObject networkObject = NetworkedObjectPool.Instance.GetFromPool(firePoint.position, firePoint.rotation);

        // Rigidbody bileþenine eriþip hýzý ayarlayýn.
        Rigidbody rb = networkObject.GetComponent<Rigidbody>();
        rb.velocity = firePoint.rotation * Vector3.forward * bulletSpeed;
    }


    [ServerRpc(RequireOwnership = false)]
    public void BulletDamageServerRpc(Vector3 collisionPoint)
    {
        if (!IsOwner) return;

        PlayTemporarySound(hurtAudio);

        Debug.Log("ServerRpc çaðrýldý, oyuncu hasar alacak!");

        Vector3 dir = (transform.position - collisionPoint).normalized;
        rb.AddForce(dir * 100f, ForceMode.Impulse); // Tepme etkisi

        Debug.Log("Oyuncunun hasar aldi");
    }


    public void Shield()
    {
        PlayTemporarySound(shieldActiveAudio);
        shield.SetActive(true);
        StartCoroutine(Coroutine());

        Debug.Log("Shield calisti");

    }

    IEnumerator Coroutine()
    {
        yield return new WaitForSeconds(10);
        shield.SetActive(false);
    }

    public void Dash()
    {
        PlayTemporarySound(dashSudio);

        isDashing = true;

        rb.AddForce(frontPoint.forward * dashForce, ForceMode.Impulse);

        Invoke(nameof(StopDash), dashDuration);
    }

    private void StopDash()
    {
        isDashing = false;
        rb.velocity = Vector3.zero; // Dash bittikten sonra hýzý sýfýrla
    }

    public void HighJump()
    {
        jumpForce = 20;
        gravityScale = 20;
    }

    public void InitialJump()
    {
        jumpForce = 10;
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
        UI_Manager.Instance.RemovePlayerNickName(gameObject);

        if (IsOwner && playerCamera != null)
        {
            Destroy(playerCamera); // Oyuncu ayrýldýðýnda kamerayý yok et
        }
    }
}

