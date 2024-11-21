using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowTrail : MonoBehaviour
{
    public TrailRenderer trailRenderer;
    public float trailOffset = -0.1f; // Trail'i y�zeye oturtmak i�in

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        InvokeRepeating("Trail", 1f, 0.1f);
    }

    void Trail()
    {
        // Trail'in snowboard alt�nda hizalanmas�
        Vector3 adjustedPosition = transform.position + Vector3.up * trailOffset;
        trailRenderer.transform.position = adjustedPosition;

        // H�zla uyumlu geni�lik
        float speed = rb.velocity.magnitude;
        trailRenderer.startWidth = Mathf.Clamp(speed * 0.05f, 0.2f, 0.8f);
        trailRenderer.endWidth = trailRenderer.startWidth * 0.5f;
    }
}
