using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using System.Threading.Tasks;

public class LobbyManager : MonoBehaviour
{
    private Lobby currentLobby;
    private float heartbeatTimer;

    public async Task CreateLobby(string lobbyName, int maxPlayers)
    {
        try
        {
            var options = new CreateLobbyOptions
            {
                IsPrivate = false
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                lobbyName,
                maxPlayers,
                options
            );

            Debug.Log($"Lobby created: {lobby.Name}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobby creation failed: {e.Message}");
        }
    }

    public async void JoinLobby(string lobbyId)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            Debug.Log($"Joined lobby: {currentLobby.Name}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Joining lobby failed: {e}");
        }
    }

    private async void Update()
    {
        // Lobby heartbeat (lobi sürekliliði saðlamak için gereklidir)
        if (currentLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                heartbeatTimer = 15f; // 15 saniyede bir yenile
                await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }
        }
    }
    public Lobby GetCurrentLobby()
    {
        return currentLobby; // Lobi bilgilerini döndür
    }
}
