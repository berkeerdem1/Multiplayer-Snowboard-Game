using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox_Rotator : MonoBehaviour
{
    public Material skyboxMaterial; 
    public float rotationSpeed = 1.0f; 

    private void Start()
    {
        InvokeRepeating("Rotate", 0, 0.02f);
    }
    void Rotate()
    {
        if (skyboxMaterial != null)
        {
            float currentRotation = skyboxMaterial.GetFloat("_Rotation");
            currentRotation += rotationSpeed * Time.deltaTime;

            skyboxMaterial.SetFloat("_Rotation", currentRotation);
        }
    }
}
