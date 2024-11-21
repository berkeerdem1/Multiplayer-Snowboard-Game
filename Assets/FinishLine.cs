using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    public bool raceFinished = false;

    private void OnTriggerEnter(Collider other)
    {
        if (raceFinished) return;

        var networkObject = other.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            ulong playerId = networkObject.OwnerClientId;

            // Yar��ta olan oyuncu mu?
            var raceManager = FindObjectOfType<Race_Manager>();
            if (raceManager.activePlayers.Contains(playerId))
            {
                Debug.Log($"Oyuncu {playerId} biti� �izgisine ula�t�!");
                raceManager.EndRace(playerId);
                raceFinished = true;
            }
        }
    }
}
