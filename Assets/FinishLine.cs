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
            ulong winnerId = networkObject.OwnerClientId;

            if (Race_Manager.Instance.activePlayers.Contains(winnerId))
            {
                Debug.Log($"Oyuncu {winnerId} bitiþ çizgisine ulaþtý!");
                //raceManager.EndRace(playerId);
                Race_Manager.Instance.CheckWinnerServerRpc(winnerId);
                Race_Manager.Instance.SetWinnerServerRpc(NetworkManager.Singleton.LocalClientId);


                raceFinished = true;
            }
        }
    }
}
