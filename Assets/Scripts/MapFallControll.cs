using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapFallControll : MonoBehaviour
{
    public Transform teleport;
    private void OnTriggerEnter(Collider other)
    {
        other.transform.position = teleport.position;   
    }
}
