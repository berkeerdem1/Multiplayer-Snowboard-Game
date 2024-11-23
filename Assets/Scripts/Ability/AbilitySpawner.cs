using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AbilitySpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] abilityPrebafs;
    [SerializeField]
    private Transform[] spawnPositions;


    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }
    }

    private void OnServerStarted()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Sunucu ba�lat�ld�, objeler spawn ediliyor.");
            SpawnObjects();
        }
    }

    private void SpawnObjects()
    {
        if (abilityPrebafs != null)
        {
            for(int i = 0; i < abilityPrebafs.Length; i++)
            {
                var spawnedObject = Instantiate(abilityPrebafs[i], spawnPositions[i].position, Quaternion.identity);
                var networkObject = spawnedObject.GetComponent<NetworkObject>();

                if (networkObject != null)
                {
                    networkObject.Spawn(); // Sunucu taraf�nda spawn i�lemi
                    Debug.Log("Obje spawn edildi.");
                }
                else
                {
                    Debug.LogError("Prefab'da NetworkObject bile�eni bulunamad�.");
                }
            }
        }
        else
        {
            Debug.LogError("Spawn edilecek prefab atanmad�.");
        }
    }

}
