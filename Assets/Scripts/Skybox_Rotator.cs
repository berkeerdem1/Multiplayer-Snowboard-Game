using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox_Rotator : MonoBehaviour
{
    public Material skyboxMaterial; // Skybox Material'in referans�
    public float rotationSpeed = 1.0f; // Rotasyon h�z� (derece/saniye)

    private void Start()
    {
        InvokeRepeating("Rotate", 0, 0.02f);
    }
    void Rotate()
    {
        if (skyboxMaterial != null)
        {
            // Mevcut rotasyon de�erini al ve art�r
            float currentRotation = skyboxMaterial.GetFloat("_Rotation");
            currentRotation += rotationSpeed * Time.deltaTime;

            // Yeni rotasyon de�erini skybox material'e uygula
            skyboxMaterial.SetFloat("_Rotation", currentRotation);
        }
    }
}
