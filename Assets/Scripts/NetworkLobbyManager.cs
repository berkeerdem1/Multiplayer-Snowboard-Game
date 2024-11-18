using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkLobbyManager : MonoBehaviour
{
    public LobbyManager lobbyManager;
    private RelayManager relayManager;

    private void Awake()
    {
        relayManager = FindFirstObjectByType<RelayManager>();
    }
    public void StartHost()
    {
        relayManager.CreateRelay();
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host started.");
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client started.");
    }
}
