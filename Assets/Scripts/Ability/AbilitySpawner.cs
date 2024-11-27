using Unity.Netcode;
using UnityEngine;

public class AbilitySpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] abilityPrebafs;
    [SerializeField] private Transform[] spawnPositions;


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
            Debug.Log("Server started, objects spawning.");
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
                    networkObject.Spawn();
                    Debug.Log("Object spawned.");
                }
                else
                {
                    Debug.LogError("NetworkObject component not found in Prefab.");
                }
            }
        }
        else
        {
            Debug.LogError("Prefab to be spawned is not assigned.");
        }
    }

}
