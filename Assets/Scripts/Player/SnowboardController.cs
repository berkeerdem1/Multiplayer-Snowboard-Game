using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SnowboardController : NetworkBehaviour
{

    [Header("MOVEMENT & SLÝDE & DRAG")]
    public float throttle = 0f; // Acceleration factor
    
    [SerializeField] private float acceleration = 500f; // Forward/Backward starting force
    [SerializeField] private float maxAcceleration = 2000f; // Max Forward force
    [SerializeField] private float steering = 100f; // Rotation speed
    [SerializeField] private float maxSpeed = 30f; // Max speed
    [SerializeField] private float drag = 0.98f; // Drifting to reduce speed
    [SerializeField] private float throttleIncreaseRate = 2f; // Acceleration factor increase rate
    [SerializeField] private float throttleDecreaseRate = 5f; // Acceleration factor decrease rate
    [SerializeField] private float brakeForce = 0.2f;
    [SerializeField] private float minDrag = 0.1f; // Min drag value
    [SerializeField] private float maxDrag = 1f; // Max drag value
    [SerializeField] private float gravityScale; // Gravity value
    [SerializeField] private float slopeSlideStrength = 1; // Slide strengTH

    [Header("SOMERSAULT")]
    private Vector3[] flipDirections = new Vector3[]
    {
        Vector3.forward,  // forward flip
        Vector3.back,     // back flip
        Vector3.left,     // left flip
        Vector3.right,    // right flip
    };
    [SerializeField] private int flipCount = 2;
   public bool SetFlippingState = false;


    [Header("JUMP")] public float jumpForce = 10;

    [Header("GROUND CONTROL")]
    [SerializeField] private float groundCheckDistance = 0.2f; // Raycast distance to check distance from ground
    [SerializeField] private LayerMask groundLayer;           //To check the ground layer only
    private bool isGrounded = false;      


    [Header("COMPONENTS")]
    [SerializeField] private Transform frontPoint;
    [SerializeField] private GameObject CameraPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private GameObject shield;



    [Header("COMPONENTS")]
    private bool controlsEnabled = true;
    private GameObject _playerCamera;
    private Rigidbody _rb;
    private GameObject _spawnObject;

    private NetworkVariable<float> _randomFloatNumber = new NetworkVariable<float>(5.5f, 
                                                                                NetworkVariableReadPermission.Everyone,
                                                                                NetworkVariableWritePermission.Owner);
    float moveInput, turnInput;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _playerCamera = Instantiate(CameraPrefab); // Create camera prefab
            _playerCamera.GetComponent<CameraFollow>().SetTarget(transform);


            // Check that you are a client
            if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("This is client");
            }

            // Check if you are connected to the server
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.Log("The client successfully connected to the server.");
            }
            else
            {
                Debug.LogError("The client is not connected to the server!");
            }
        }

        if (IsServer)
        {
            Debug.Log("Bu obje sunucuda spawn oldu.");
        }
        else
        {
            Debug.Log("This object was spawned on the client side.");
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        shield.SetActive(false);
        InvokeRepeating("Movement", 0, 0.02f);
    }

    private void Update()
    {
        moveInput = Input.GetAxis("Vertical"); 
        turnInput = Input.GetAxis("Horizontal"); // Right/left movement

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _rb.velocity *= (1f - brakeForce * Time.fixedDeltaTime);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _rb.velocity *= (1f - brakeForce * Time.fixedDeltaTime);
        }

        if (Input.GetKeyUp(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    bool IsMoving()
    {
        return Mathf.Abs(_rb.velocity.magnitude) > 0.1f;
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

        //Check the acceleration factor
        if (moveInput > 0 && isGrounded) // Acceleration is increased only when on the ground
        {
            throttle += throttleIncreaseRate * Time.fixedDeltaTime;
        }
        else if (moveInput == 0 && isGrounded) // If we didn't brake, the speed drop would be slow.
        {
            throttle -= throttleDecreaseRate * 0.5f * Time.fixedDeltaTime;
        }
        else
        {
            throttle -= throttleDecreaseRate * Time.fixedDeltaTime;
        }

        throttle = Mathf.Clamp(throttle, 0f, 1f); // Limit the acceleration factor between 0 and 1

        //Debug.Log($"Current Velocity: {rb.velocity.magnitude}");

        Turn();
        Gravity();
        //Dragging(moveInput);
        MovementByFrontPoint();
        Somersault();
        Slope();
        CheckGround();

        //Debug.DrawRay(firePoint.position, firePoint.forward * 5, Color.red, 2f);


    }

    public bool CheckGround() // Ground control with raycast
    {
        // Raycast 
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            return true;
        }   

        // 2. Collider
        return false; // If raycast doesn't find anything, return false
    }

    private void Gravity()
    {
        if (!isGrounded)
        {
            _rb.AddForce(Vector3.down * gravityScale * _rb.mass, ForceMode.Acceleration); // Gravity effect
        }
        else gravityScale = 10;
    } // Gravity effect

    private void Turn() // Right/ left turn
    {
        float turn = turnInput * steering * Time.fixedDeltaTime;
        transform.Rotate(0, turn, 0);
    }

    private void Dragging(float MoveInput) // Dynamic Drag
    {
        if (isGrounded && MoveInput == 0)
        {
            _rb.velocity *= Mathf.Lerp(1f, drag, Time.fixedDeltaTime); // Softer deceleration
        }

        float dynamicDrag = Mathf.Lerp(maxDrag, minDrag, _rb.velocity.magnitude / maxSpeed); 
        _rb.velocity *= Mathf.Clamp01(1f - dynamicDrag * Time.fixedDeltaTime);
    }

    private void MovementByFrontPoint()
    {
        if (frontPoint != null) 
        {
            Vector3 directionToMove = (frontPoint.position - transform.position).normalized; // Direction towards the front point
            float currentAcceleration = Mathf.Lerp(0, maxAcceleration, throttle); // Take acceleration into account
            Vector3 forwardForce = directionToMove * moveInput * currentAcceleration * Time.fixedDeltaTime;

            // Check maximum speed limit
            if (_rb.velocity.magnitude < maxSpeed || moveInput == 0) // Even if it is left idle, it will slip
            {
                _rb.AddForce(forwardForce, ForceMode.Acceleration);
            }
        }
    } // Apply the movement force according to the front point direction

    private void Somersault() // Flip Input
    {
        if (/*Vector3.Dot(transform.up, Vector3.up) < 0.5f */ flipCount > 0)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                PerformRandomFlip();
                flipCount -= 1;
            }
        }

        if (isGrounded)
        {
            SetFlippingState = false;
            flipCount = 2;
        }
        else
        {
            SetFlippingState = true;

        }
    }

    void PerformRandomFlip()
    {
        // Random direction
        Vector3 randomDirection = flipDirections[Random.Range(0, flipDirections.Length)];

        _rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
        _rb.AddTorque(randomDirection * 20f, ForceMode.Impulse);
    } // Random Flip addforce

    private void Slope()
    {
        if (isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
            {
                Vector3 groundNormal = hit.normal; // Normal vector of the ground
                Vector3 slopeDirection = Vector3.Cross(transform.right, groundNormal); // Slope direction

                float slopeAngle = Vector3.Angle(groundNormal, Vector3.up); // Angle of slope
                if (slopeAngle > 0.1f) // Provides sliding even if there is a slight slope
                {
                    float slopeFactor = Mathf.Clamp01(slopeAngle / 45f); // Maximum slip at 45 degree slope
                    Vector3 slopeForce = slopeDirection.normalized * slopeFactor * slopeSlideStrength;

                    _rb.AddForce(slopeForce, ForceMode.Acceleration);
                }
            }
        }
    } // Slope Shift

    private void Jump()
    {
        if (!IsOwner) return;

        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        gravityScale = 20;
    }


    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > 0.1f) // If the surface is upwards
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
    private void TestingServerRpc()  // If the host is the owner, it writes to the console
    {
        Debug.Log(OwnerClientId + " Testing Server Rpc");
    }

    [ClientRpc]
    private void TestingClientRpc() //  If the client, it writes to the console
    {
        Debug.Log(OwnerClientId + " Testing Client Rpc");
    }

    private void OnDestroy()
    {
        UI_Manager.Instance.RemovePlayerNickName(gameObject);

        if (IsOwner && _playerCamera != null)
        {
            Destroy(_playerCamera); // Destroy camera when player leaves
        }
    }
}

