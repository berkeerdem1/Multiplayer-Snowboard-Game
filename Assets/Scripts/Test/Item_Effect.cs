using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Item_Effect : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Oyuncu mu kontrol et
        if (other.CompareTag("Player"))
        {
            // NetworkObject referansý al
            NetworkObject playerNetworkObject = other.GetComponentInParent<NetworkObject>();
            if (playerNetworkObject != null)
            {
                // Etkiyi server üzerinden tetikle
                ApplyEffectServerRpc(playerNetworkObject.NetworkObjectId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyEffectServerRpc(ulong playerNetworkId)
    {
        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkId];

        if (playerNetworkObject != null)
        {
            // Etkiyi client'e gönder
            ApplyEffectClientRpc(playerNetworkId);
        }
    }

    [ClientRpc]
    private void ApplyEffectClientRpc(ulong playerNetworkId)
    {
        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkId];

        if (playerNetworkObject != null)
        {
            // Oyuncuya etki uygula
            SnowboardController player = playerNetworkObject.GetComponent<SnowboardController>();
            if (player != null)
            {
                player.ApplyItemEffect();
            }
        }
    }
}
