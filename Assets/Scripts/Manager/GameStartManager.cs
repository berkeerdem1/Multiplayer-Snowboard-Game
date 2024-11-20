using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class GameStartManager : MonoBehaviour
{
    private LobbyManager lobbyManager;

    private void Start()
    {
        lobbyManager = FindObjectOfType<LobbyManager>();
    }
    public async void StartGame()
    {
        Lobby currentLobby = lobbyManager.GetCurrentLobby();

        if (currentLobby != null)
        {
            try
            {
                await LobbyService.Instance.UpdateLobbyAsync(
                    currentLobby.Id,
                    new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject>
                        {
                        { "GameStarted", new DataObject(DataObject.VisibilityOptions.Public, "true") }
                        }
                    });

                Debug.Log("Game started!");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to start game: {e}");
            }
        }
        else
        {
            Debug.LogError("No lobby found to start the game.");
        }
    }
}
