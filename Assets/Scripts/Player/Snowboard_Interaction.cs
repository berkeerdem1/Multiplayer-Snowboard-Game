using UnityEngine;

public class Snowboard_Interaction : MonoBehaviour
{
    [SerializeField] private Transform characterTransform;
    [SerializeField] private float rotationLerpSpeed = 5f;
    [SerializeField] private Vector3 playerOffset = new Vector3(0, 0.5f, 0); // Player's starting position relative to the snowboard
    [SerializeField] private Vector3 playerShieldOffset = new Vector3(0, 1f, 0); // Shiled's starting position relative to the snowboard
    [SerializeField] private Transform shieldPos; // Shield transform

    private SnowboardController controller;

    private void Awake()
    {
        controller = GetComponent<SnowboardController>();
    }

    private void FixedUpdate()
    {
        if (characterTransform != null)
        {
            // 1. Align position to center of snowboard & shield.
            characterTransform.position = transform.TransformPoint(playerOffset);
            shieldPos.position = transform.TransformPoint(playerShieldOffset);

            // 2. Align rotaiton to center of snowboard & shield.
            Quaternion targetRotation = transform.rotation;
            characterTransform.rotation = Quaternion.Lerp(characterTransform.rotation, targetRotation, Time.fixedDeltaTime * rotationLerpSpeed);
            shieldPos.rotation = Quaternion.Lerp(shieldPos.rotation, targetRotation, Time.fixedDeltaTime * rotationLerpSpeed);
        }
    }
}
