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
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bullerPrefab;
    [SerializeField] private float bulletSpeed;

    private Vector3[] flipDirections = new Vector3[]
    {
        Vector3.forward,  // �ne takla
        Vector3.back,     // Arkaya takla
        Vector3.left,     // Sola takla
        Vector3.right,    // Sa�a takla
    };


    [SerializeField] private GameObject shield;

    [SerializeField] private float dashForce = 10f; // Dash g�c�
    [SerializeField] private float dashDuration = 0.2f; // Dash s�resi
    private bool isDashing = false;

    [SerializeField]
    private float jumpForce = 10;

    [SerializeField] private AudioClip dashSudio, shieldActiveAudio, hurtAudio;
    [SerializeField] private AudioSource audio; // Motor sesi AudioSource
    [SerializeField] private float maxVolume = 1f;    // Maksimum ses seviyesi
    [SerializeField] private float volumeChangeRate = 2f; // Ses seviyesinin de�i�im h�z�

    [SerializeField] private float playInterval = 0.2f; // Sesin ne s�kl�kta �al�naca�� (saniye)
    private float currentVolume; // Anl�k ses seviyesi

    private float lastPlayTime; // Son ses �alma zaman�


    [SerializeField] private float groundCheckDistance = 0.2f; // Yerden mesafeyi kontrol etmek i�in raycast mesafesi
    [SerializeField] private LayerMask groundLayer;           // Yaln�zca zemin katman�n� kontrol etmek i�in
    private bool isGrounded = false;        // Yerle temas durumunu saklar

    public bool SetFlippingState = false;

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


            // �stemci oldu�unuzu kontrol edin
            if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("Bu bir istemci.");
            }

            // Sunucuya ba�l� olup olmad���n�z� kontrol edin
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.Log("�stemci ba�ar�yla sunucuya ba�l�.");
            }
            else
            {
                Debug.LogError("�stemci sunucuya ba�l� de�il!");
            }
        }

        if (IsServer)
        {
            Debug.Log("Bu obje sunucuda spawn oldu.");
        }
        else
        {
            Debug.Log("Bu obje istemci taraf�nda spawn oldu.");
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
        // 1. Kullan�c� girdilerini al
        moveInput = Input.GetAxis("Vertical"); // �leri/Geri hareket
        turnInput = Input.GetAxis("Horizontal"); // Sa�a/Sola d�n��

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
        // �tem etkisini burada uygula
        Debug.Log("Item etkisi uyguland�!");
        // �rnek: Can artt�rma
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
        // 1. Raycast y�ntemi
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            return true;
        }   

        // 2. Collider y�ntemi
        return false; // E�er raycast bir �ey bulamazsa false d�nd�r
    }

    private void Gravity()
    {
        // 5. Yer�ekimi etkisi
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * gravityScale * rb.mass, ForceMode.Acceleration); // Yer�ekimi etkisi
        }
        else gravityScale = 10;
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
        if (/*Vector3.Dot(transform.up, Vector3.up) < 0.5f &&*/ CheckGround()) // Araba yakla��k olarak ters d�nm��se
        {
            // Kullan�c� belirli bir tu�a bast���nda takla atmay� tetikle
            if (Input.GetKeyDown(KeyCode.LeftShift)) // "R" tu�u d�zelme i�in �rnek
            {
                PerformRandomFlip();
            }
        }

        // Zeminle temas sa�land���nda takla durumunu devre d��� b�rak
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
        // Rastgele bir y�n se�
        Vector3 randomDirection = flipDirections[Random.Range(0, flipDirections.Length)];

        // Yukar� do�ru bir z�plama kuvveti uygula
        rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);

        // Rastgele bir y�nde tork uygula
        rb.AddTorque(randomDirection * 20f, ForceMode.Impulse);
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
            // Ses seviyesini throttle ile ili�kilendir
            audio.volume = Mathf.Lerp(audio.volume, throttle * maxVolume, volumeChangeRate * Time.deltaTime);

            // E�er ses �alm�yorsa ba�lat
            if (!audio.isPlaying)
            {
                audio.Play();
            }
        }
        else
        {
            // Ses seviyesini kademeli olarak azalt ve s�f�r oldu�unda durdur
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

        // �alan sesi durdur
        audio.Stop();

        // PlayOneShot ile ge�ici sesi �al
        audio.PlayOneShot(tempClip);

        // Ge�ici sesin uzunlu�u kadar bekleyip eski sesi geri y�kle
        StartCoroutine(RestoreOriginalSound(originalClip, wasPlaying, tempClip.length));
    }

    private IEnumerator RestoreOriginalSound(AudioClip originalClip, bool wasPlaying, float delay)
    {
        // Ge�ici sesin s�resi kadar bekle
        yield return new WaitForSeconds(delay);

        // Orijinal sesi geri y�kle
        audio.clip = originalClip;

        // E�er eski ses �al�yorsa devam ettir
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




    public void Shoot()    
    {
        // Mermiyi do�ru bir rotasyonla spawnlay�n.
        //GameObject newBullet =Instantiate(bullerPrefab, firePoint.position, firePoint.rotation);
        //NetworkObject networkObject = newBullet.GetComponent<NetworkObject>();
        //networkObject.Spawn();
        NetworkObject networkObject = NetworkedObjectPool.Instance.GetFromPool(firePoint.position, firePoint.rotation);

        // Rigidbody bile�enine eri�ip h�z� ayarlay�n.
        Rigidbody rb = networkObject.GetComponent<Rigidbody>();
        rb.velocity = firePoint.rotation * Vector3.forward * bulletSpeed;
    }


    [ServerRpc(RequireOwnership = false)]
    public void BulletDamageServerRpc(Vector3 collisionPoint)
    {
        if (!IsOwner) return;

        PlayTemporarySound(hurtAudio);

        Debug.Log("ServerRpc �a�r�ld�, oyuncu hasar alacak!");

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
        rb.velocity = Vector3.zero; // Dash bittikten sonra h�z� s�f�rla
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
            Destroy(playerCamera); // Oyuncu ayr�ld���nda kameray� yok et
        }
    }
}

