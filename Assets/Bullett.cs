using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullett : NetworkBehaviour
{

    private float lifeTimer = 8F;


    private void Start()
    {
        StartCoroutine(Lifetime());
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!IsServer) return; // Çarpýþmayý yalnýzca sunucu kontrol eder

        var player = collision.gameObject.GetComponentInParent<SnowboardController>();

        if (player != null)
        {
            Debug.Log("Mermi: Oyuncuya Deðdim!");
            Vector3 hitPoint = collision.ClosestPoint(transform.position);
            player.BulletDamageServerRpc(hitPoint); // Oyuncuya hasar ver

            Debug.Log("Mermi: Oyuncuya hasar verme fonksiyonunu cagirdim!");

            //ReturnToPool();

            if (GetComponent<NetworkObject>().IsSpawned)
            {
                GetComponent<NetworkObject>().Despawn(); // NetworkObject'in despawn edilmesi
            }
            else
            {
                Destroy(gameObject); // Fallback olarak normal yok etme
            }
        }

        if (collision.gameObject.CompareTag("Shield")) // Çarptýðý nesne bir kalkan ise
        {
            Debug.Log("Mermi: Kalkana Deðdim!");
            //ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        NetworkedObjectPool.Instance.ReturnToPool(GetComponent<NetworkObject>());
    }

    IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(lifeTimer);

        //ReturnToPool();

        if (GetComponent<NetworkObject>().IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn(); // NetworkObject'in despawn edilmesi
        }
        else
        {
            Destroy(gameObject); // Fallback olarak normal yok etme
        }
    }

}
