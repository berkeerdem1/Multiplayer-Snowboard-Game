using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    float playerGroundTimer = 0f;

    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10); // Camera follow position
    [SerializeField] private float followSpeed = 10f; 
    [SerializeField] private float heightLock = 5f;
    [SerializeField] private float smoothness = 0.125f; 

    private SnowboardController snowboard;
    private Transform target; // player


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
            return;
        }

        Vector3 desiredPosition ;

        // If the snowboarder is flipping, just follow the position
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

        // Camera target position 
        desiredPosition.y = Mathf.Lerp(transform.position.y, target.position.y + offset.y, followSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Maintain the angle at which the camera views the snowboard
        Vector3 lookAtPosition = target.position;
        lookAtPosition.y = transform.position.y; // Keeps the camera stable on the Y axis
        transform.LookAt(lookAtPosition);
    }
}
