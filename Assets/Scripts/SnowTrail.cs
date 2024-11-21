using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowTrail : MonoBehaviour
{
    public TrailRenderer trailRenderer;
    public float trailOffset = -0.1f; // Trail'i yüzeye oturtmak için

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        InvokeRepeating("Trail", 1f, 0.1f);
    }

    void Trail()
    {
        // Trail'in snowboard altýnda hizalanmasý
        Vector3 adjustedPosition = transform.position + Vector3.up * trailOffset;
        trailRenderer.transform.position = adjustedPosition;

        // Hýzla uyumlu geniþlik
        float speed = rb.velocity.magnitude;
        trailRenderer.startWidth = Mathf.Clamp(speed * 0.05f, 0.2f, 0.8f);
        trailRenderer.endWidth = trailRenderer.startWidth * 0.5f;
    }
}
