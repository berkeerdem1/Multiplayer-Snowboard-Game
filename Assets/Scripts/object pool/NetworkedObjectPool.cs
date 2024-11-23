using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkedObjectPool : MonoBehaviour
{
    public static NetworkedObjectPool Instance { get; private set; }

    [SerializeField] private NetworkObject prefab; // NetworkObject mermi prefab'ý
    [SerializeField] private int poolSize = 20;

    private Queue<NetworkObject> pool = new Queue<NetworkObject>();

    void Awake()
    {
        Instance = this;

        // Havuzu doldur
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

            // Eðer havuzdan alýnan obje yok edilmiþse, yeni bir tane oluþtur
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

        // Nesnenin pozisyonunu ve rotasyonunu ayarla
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);

        // Eðer NetworkObject spawn edilmemiþse, spawn et
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
            newObj.Spawn(); // Sadece server'da spawn et
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
            obj.Despawn(); // Network spawn'ý kapat
        }

        obj.gameObject.SetActive(false); // Nesneyi devre dýþý býrak
        pool.Enqueue(obj);
    }
}
