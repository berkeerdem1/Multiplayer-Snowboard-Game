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

            // Yarýþta olan oyuncu mu?
            var raceManager = FindObjectOfType<Race_Manager>();
            if (raceManager.activePlayers.Contains(playerId))
            {
                Debug.Log($"Oyuncu {playerId} bitiþ çizgisine ulaþtý!");
                raceManager.EndRace(playerId);
                raceFinished = true;
            }
        }
    }
}
