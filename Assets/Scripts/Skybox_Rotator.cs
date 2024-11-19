using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox_Rotator : MonoBehaviour
{
    public Material skyboxMaterial; // Skybox Material'in referansý
    public float rotationSpeed = 1.0f; // Rotasyon hýzý (derece/saniye)

    private void Start()
    {
        InvokeRepeating("Rotate", 0, 0.02f);
    }
    void Rotate()
    {
        if (skyboxMaterial != null)
        {
            // Mevcut rotasyon deðerini al ve artýr
            float currentRotation = skyboxMaterial.GetFloat("_Rotation");
            currentRotation += rotationSpeed * Time.deltaTime;

            // Yeni rotasyon deðerini skybox material'e uygula
            skyboxMaterial.SetFloat("_Rotation", currentRotation);
        }
    }
}
