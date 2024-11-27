using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkedObjectPool : MonoBehaviour
{
    public static NetworkedObjectPool Instance { get; private set; }

    [SerializeField] private NetworkObject prefab; // NetworkObject bullet prefab
    [SerializeField] private int poolSize = 20; // Pool networkobject limit

    private Queue<NetworkObject> pool = new Queue<NetworkObject>();

    void Awake()
    {
        Instance = this;

        // Fill the pool
        for (int i = 0; i < poolSize; i++)
        {
            NetworkObject obj = Instantiate(prefab);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    private void Start()
    {
        ValidatePool();
    }

    private void ValidatePool()
    {
        Queue<NetworkObject> validPool = new Queue<NetworkObject>();

        foreach (var obj in pool)
        {
            if (obj != null)
            {
                validPool.Enqueue(obj);
            }
            else
            {
                Debug.LogWarning("Destroyed object found in pool. Removing...");
            }
        }

        pool = validPool;
    }

    public NetworkObject GetFromPool(Vector3 position, Quaternion rotation)
    {
        NetworkObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();

            // If the object taken from the pool is destroyed, create a new one
            if (obj == null)
            {
                Debug.LogWarning("Destroyed object found in pool. Creating a new one...");
                obj = CreateNewObject();
            }
        }
        else
        {
            Debug.LogWarning("Pool is empty! Creating a new object...");
            obj = CreateNewObject();
        }

        // Object position ve rotasion set
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);

        // If NetworkObject is not spawned, spawn it
        if (NetworkManager.Singleton.IsServer && !obj.IsSpawned)
        {
            obj.Spawn();
        }

        return obj;
    }


    private NetworkObject CreateNewObject()
    {
        NetworkObject newObj = Instantiate(prefab);
        newObj.gameObject.SetActive(false);

        if (NetworkManager.Singleton.IsServer)
        {
            newObj.Spawn(); // Only spawn on server
        }

        return newObj;
    }

    public void ReturnToPool(NetworkObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("Attempted to return a null object to the pool.");
            return;
        }

        if (NetworkManager.Singleton.IsServer && obj.IsSpawned)
        {
            obj.Despawn(); // Object despawn
        }

        obj.gameObject.SetActive(false); // Object disabled
        pool.Enqueue(obj);
    }
}
